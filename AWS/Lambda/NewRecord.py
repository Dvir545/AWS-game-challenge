import json
import boto3
from datetime import datetime
from boto3.dynamodb.conditions import Key
from decimal import Decimal
import os

# Initialize DynamoDB client
dynamodb = boto3.resource('dynamodb')
table_name = os.environ['TABLE_NAME'] 
table = dynamodb.Table(table_name)

def safe_decimal_conversion(value):
    """Safely convert a value to Decimal, handling various input types."""
    try:
        if isinstance(value, (int, float)):
            return Decimal(str(value))
        elif isinstance(value, str):
            # Remove any whitespace and validate the string
            cleaned_value = value.strip()
            if not cleaned_value:
                raise ValueError("Empty string")
            return Decimal(cleaned_value)
        elif isinstance(value, Decimal):
            return value
        else:
            raise ValueError(f"Unsupported type for conversion: {type(value)}")
    except Exception as e:
        raise ValueError(f"Failed to convert {value} to Decimal: {str(e)}")

def lambda_handler(event, context):
    try:
        # Handle both direct request body and API Gateway event
        if isinstance(event, str):
            request_body = json.loads(event)
        elif isinstance(event, dict):
            if 'body' in event:
                request_body = json.loads(event['body']) if isinstance(event['body'], str) else event['body']
            else:
                request_body = event
                
        print(f"Raw request body: {request_body}")
                
        player_name = request_body.get('playerName')
        days_survived = request_body.get('daysSurvived')
        time_taken = request_body.get('timeTaken')
        
        print(f"Extracted values - playerName: {player_name}, days: {days_survived}, time: {time_taken}")
        
        # Validate input
        if not player_name:
            raise ValueError("playerName is required")
        if days_survived is None:
            raise ValueError("daysSurvived is required")
        if time_taken is None:
            raise ValueError("timeTaken is required")
            
        # Convert time_taken to Decimal using safe conversion
        time_taken_decimal = safe_decimal_conversion(time_taken)
        days_survived = int(days_survived)  # Ensure days is an integer
        
        print(f"Converted values - days: {days_survived}, time: {time_taken_decimal}")

        # Query existing records for this player
        query_response = table.query(
            KeyConditionExpression=Key('playerName').eq(player_name)
        )
        
        print(f"Found existing records: {query_response['Items']}")
        
        should_update = False
        
        if not query_response['Items']:
            # No existing record, create new one
            should_update = True
            print("No existing records found, will create new")
        else:
            # Find the best record for this player
            best_record = min(query_response['Items'], 
                            key=lambda x: (-int(x['Days']), safe_decimal_conversion(x['timeTaken'])))
            
            print(f"Best existing record: {best_record}")
            
            best_days = int(best_record['Days'])
            best_time = safe_decimal_conversion(best_record['timeTaken'])
            
            if (days_survived > best_days or 
                (days_survived == best_days and time_taken_decimal < best_time)):
                should_update = True
                print("New score is better, will update")
                
                # Delete all existing records for this player
                for record in query_response['Items']:
                    table.delete_item(
                        Key={
                            'playerName': player_name,
                            'Days': record['Days']
                        }
                    )
                print("Deleted existing records")

        if should_update:
            # Create new record
            timestamp = datetime.now().isoformat()
            new_record = {
                'playerName': player_name,
                'Days': str(days_survived),  # Sort key must be string
                'timeTaken': time_taken_decimal,
                'timestamp': timestamp
            }
            
            print(f"Creating new record: {new_record}")
            table.put_item(Item=new_record)

            return {
                'statusCode': 200,
                'headers': {
                    'Content-Type': 'application/json',
                    'Access-Control-Allow-Origin': '*'
                },
                'body': json.dumps({
                    'message': 'Score updated successfully',
                    'record': {
                        'playerName': player_name,
                        'Days': str(days_survived),
                        'timeTaken': float(time_taken_decimal),  # Convert back to float for JSON response
                        'timestamp': timestamp
                    }
                }, default=str)  # Handle any remaining Decimal serialization
            }
        else:
            print("Existing score is better, no update needed")
            return {
                'statusCode': 200,
                'headers': {
                    'Content-Type': 'application/json',
                    'Access-Control-Allow-Origin': '*'
                },
                'body': json.dumps({
                    'message': 'Existing score is better, no update needed'
                })
            }
                
    except ValueError as ve:
        print(f'Validation error: {str(ve)}')
        return {
            'statusCode': 400,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                'message': 'Invalid input',
                'error': str(ve)
            })
        }
    except Exception as e:
        print(f'Error detail: {str(e)}')
        import traceback
        print(f'Stack trace: {traceback.format_exc()}')
        return {
            'statusCode': 500,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                'message': 'Internal server error',
                'error': str(e)
            })
        }
