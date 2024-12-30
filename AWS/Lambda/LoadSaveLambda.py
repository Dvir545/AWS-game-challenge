import json
import boto3
import os
import logging
from botocore.exceptions import ClientError
from decimal import Decimal

# Set up logging
logger = logging.getLogger()
logger.setLevel(logging.INFO)

# Initialize DynamoDB client
dynamodb = boto3.resource('dynamodb')
table_name = os.environ['TABLE_NAME']
table = dynamodb.Table(table_name)

class DecimalEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, Decimal):
            return float(obj)
        return super(DecimalEncoder, self).default(obj)

def lambda_handler(event, context):
    """
    Lambda handler for loading game data from DynamoDB
    """
    try:
        # Log the incoming event
        logger.info(f"Processing event: {json.dumps(event)}")
        
        # Handle both direct invocation and API Gateway events
        if isinstance(event, str):
            request_data = json.loads(event)
        else:
            request_data = event

        # Extract username
        if 'username' not in request_data:
            return {
                'statusCode': 400,
                'body': json.dumps({
                    'message': 'Username is required',
                    'gameData': None
                })
            }
            
        username = request_data['username']
        
        # Get item from DynamoDB
        response = table.get_item(
            Key={
                'username': username
            }
        )
        
        # Check if item exists
        if 'Item' not in response:
            return {
                'statusCode': 200,
                'body': json.dumps({
                    'message': 'No save data found',
                    'gameData': None
                })
            }

        # Get the game data from the DynamoDB item
        saved_data = response['Item'].get('gameData', {})
        
        return {'gameData': saved_data}
        
    except ClientError as e:
        logger.error(f"DynamoDB error: {str(e)}")
        return {
            'statusCode': 500,
            'body': json.dumps({
                'message': 'Failed to load game data',
                'gameData': None
            })
        }
        
    except Exception as e:
        logger.error(f"Unexpected error: {str(e)}")
        logger.error(f"Event data: {json.dumps(event)}")
        return {
            'statusCode': 500,
            'body': json.dumps({
                'message': f'Internal server error: {str(e)}',
                'gameData': None
            })
        }
