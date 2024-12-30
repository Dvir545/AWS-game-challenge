import json
import boto3
import os
import logging
from botocore.exceptions import ClientError
from typing import Dict, List, Any, Optional
from decimal import Decimal

# Set up logging
logger = logging.getLogger()
logger.setLevel(logging.INFO)

# Initialize DynamoDB client
dynamodb = boto3.resource('dynamodb')

# Use environment variables for sensitive information
table_name = os.environ.get('DYNAMODB_TABLE_NAME')
if not table_name:
    raise ValueError("DYNAMODB_TABLE_NAME environment variable is not set")

table = dynamodb.Table(table_name)

def float_to_decimal(obj):
    """Convert float values to Decimal for DynamoDB"""
    if isinstance(obj, dict):
        return {k: float_to_decimal(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [float_to_decimal(v) for v in obj]
    elif isinstance(obj, float):
        return Decimal(str(obj))
    return obj

def validate_score_info(score_info: Optional[Dict[str, Any]]) -> bool:
    """Validates the score info structure"""
    if score_info is None:
        return False
    return all(field in score_info for field in ['daysSurvived', 'secondsSurvived'])

def validate_game_data(game_data: Optional[Dict[str, Any]]) -> bool:
    """Validates the complete game data structure"""
    if game_data is None:
        return False
        
    required_fields = [
        'username', 'TotalGamesPlayed', 'ConsecutiveGamesPlayed',
        'KilledLastGameBy', 'LastGameScore', 'HighScore'
    ]
    
    if not all(field in game_data for field in required_fields):
        logger.warning(f"Missing required fields in game_data. Found: {list(game_data.keys())}")
        return False
        
    # Validate nested score structures
    if not validate_score_info(game_data.get('LastGameScore')):
        logger.warning("Invalid LastGameScore structure")
        return False
    if not validate_score_info(game_data.get('HighScore')):
        logger.warning("Invalid HighScore structure")
        return False
        
    # CurrentGameState can be null - no validation needed
    return True

def lambda_handler(event, context):
    """
    Lambda handler for saving game data to DynamoDB
    """
    try:
        # Log the incoming event
        logger.info(f"Processing event: {json.dumps(event)}")

        # Handle both direct invocation and API Gateway events
        if isinstance(event, str):
            request_data = json.loads(event)
        else:
            request_data = event

        logger.info(f"Parsed request data: {json.dumps(request_data)}")
            
        # Extract username and game data
        if 'username' not in request_data:
            return {
                'statusCode': 400,
                'headers': {
                    'Content-Type': 'application/json',
                    'Access-Control-Allow-Origin': '*'
                },
                'body': json.dumps({'error': 'Username is required'})
            }
            
        username = request_data['username']
        game_data = request_data.get('gameData', {})
        
        # Validate game data structure
        if not validate_game_data(game_data):
            return {
                'statusCode': 400,
                'headers': {
                    'Content-Type': 'application/json',
                    'Access-Control-Allow-Origin': '*'
                },
                'body': json.dumps({'error': 'Invalid game data structure'})
            }
        
        # Convert float values to Decimal
        game_data_decimal = float_to_decimal(game_data)
        
        # Prepare item for DynamoDB
        item = {
            'username': username,
            'gameData': game_data_decimal,
            'lastUpdated': context.get_remaining_time_in_millis()
        }
        
        # Save to DynamoDB
        table.put_item(Item=item)
        
        logger.info(f"Successfully saved game data for user: {username}")
        
        return {
            'statusCode': 200,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                'message': 'Game saved successfully',
                'username': username
            })
        }
        
    except ClientError as e:
        logger.error(f"DynamoDB error: {str(e)}")
        return {
            'statusCode': 500,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                'error': 'Failed to save game data',
                'details': str(e)
            })
        }
        
    except json.JSONDecodeError as e:
        logger.error(f"JSON parsing error: {str(e)}")
        return {
            'statusCode': 400,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                'error': 'Invalid JSON format',
                'details': str(e)
            })
        }
        
    except Exception as e:
        logger.error(f"Unexpected error: {str(e)}")
        logger.error(f"Event data: {json.dumps(event)}")
        return {
            'statusCode': 500,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                'error': 'Internal server error',
                'details': str(e)
            })
        }
