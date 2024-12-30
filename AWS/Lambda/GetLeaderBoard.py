import json
import boto3
from decimal import Decimal
from boto3.dynamodb.conditions import Key
import os

# Initialize DynamoDB client
dynamodb = boto3.resource('dynamodb')
table_name = os.environ['TABLE_NAME']  # Get the table name from environment variables
table = dynamodb.Table(table_name)

# Custom JSON encoder to handle Decimal types
class DecimalEncoder(json.JSONEncoder):
    def default(self, obj):
        if isinstance(obj, Decimal):
            return float(obj)
        return super(DecimalEncoder, self).default(obj)

def lambda_handler(event, context):
    try:
        # Scan the table to get all records
        response = table.scan()
        items = response['Items']
        
        # Continue scanning if we haven't received all items
        while 'LastEvaluatedKey' in response:
            response = table.scan(ExclusiveStartKey=response['LastEvaluatedKey'])
            items.extend(response['Items'])
        
        # Convert items to the format needed by the scoreboard
        scoreboard_items = []
        for item in items:
            scoreboard_items.append({
                'playerName': item['playerName'],
                'Days': int(item['Days']),
                'timeTaken': item['timeTaken']
            })
        
        # Sort by Days (descending) and then by time taken (ascending)
        sorted_items = sorted(
            scoreboard_items,
            key=lambda x: (-int(x['Days']), x['timeTaken'])
        )
        
        # Take top 50 scores only 
        top_scores = sorted_items[:50]
        
        return {
            'statusCode': 200,
            'headers': {
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Credentials': True,
                'Content-Type': 'application/json'
            },
            'body': json.dumps({
                'scores': top_scores
            }, cls=DecimalEncoder)
        }
        
    except Exception as e:
        return {
            'statusCode': 500,
            'headers': {
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Credentials': True,
                'Content-Type': 'application/json'
            },
            'body': json.dumps({
                'error': f'Server error: {str(e)}'
            })
        }
