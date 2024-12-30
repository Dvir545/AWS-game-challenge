import json
import boto3
import os
import hmac
import base64
import hashlib
import re
from botocore.exceptions import ClientError

cognito = boto3.client('cognito-idp')
dynamodb = boto3.resource('dynamodb')
users_table = dynamodb.Table('Users')

# Fetch sensitive data from environment variables
USER_POOL_ID = os.getenv('USER_POOL_ID')
CLIENT_ID = os.getenv('CLIENT_ID')
CLIENT_SECRET = os.getenv('CLIENT_SECRET')

def is_email(text):
    # Email regex pattern
    pattern = r"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\\\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*)@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])"
    return re.match(pattern, text) is not None

def get_secret_hash(username):
    msg = username + CLIENT_ID
    dig = hmac.new(
        str(CLIENT_SECRET).encode('utf-8'),
        msg=str(msg).encode('utf-8'),
        digestmod=hashlib.sha256
    ).digest()
    return base64.b64encode(dig).decode()

def create_response(status_code, body):
    return {
        'statusCode': status_code,
        'headers': {
            'Content-Type': 'application/json',
            'Access-Control-Allow-Origin': '*'
        },
        'body': json.dumps(body)
    }

def get_user_by_email(email):
    try:
        response = users_table.scan(
            FilterExpression='email = :email',
            ExpressionAttributeValues={':email': email}
        )
        items = response.get('Items', [])
        return items[0]['username'] if items else None
    except Exception as e:
        print(f"DynamoDB error scanning by email: {str(e)}")
        return None

def get_user_email(username):
    try:
        response = users_table.get_item(
            Key={'username': username}
        )
        if 'Item' not in response:
            return None
        return response['Item'].get('email')
    except Exception as e:
        print(f"DynamoDB error: {str(e)}")
        return None

def handle_signin(username, password):
    try:
        print(f"Starting signin process for: {username}")

        if not username or not password:
            return create_response(400, {
                'success': False,
                'message': 'Username and password are required'
            })

        # Handle email format
        if is_email(username):
            email = username
            username = get_user_by_email(email)
            if not username:
                return create_response(404, {
                    'success': False,
                    'message': 'Email not found'
                })
        else:
            email = get_user_email(username)
            if not email:
                return create_response(404, {
                    'success': False,
                    'message': 'Username not found'
                })

        secret_hash = get_secret_hash(email)

        response = cognito.initiate_auth(
            ClientId=CLIENT_ID,
            AuthFlow='USER_PASSWORD_AUTH',
            AuthParameters={
                'USERNAME': email,
                'PASSWORD': password,
                'SECRET_HASH': secret_hash
            }
        )

        return create_response(200, {
            'success': True,
            'message': 'Successfully signed in',
            'username': username,
            'tokens': {
                'accessToken': response['AuthenticationResult']['AccessToken'],
                'refreshToken': response['AuthenticationResult']['RefreshToken'],
                'idToken': response['AuthenticationResult']['IdToken']
            }
        })

    except ClientError as e:
        error_code = e.response['Error']['Code']
        error_message = e.response['Error']['Message']
        print(f"Cognito error: {error_code} - {error_message}")

        if error_code == 'NotAuthorizedException':
            return create_response(401, {
                'success': False,
                'message': 'Incorrect username or password'
            })
        elif error_code == 'UserNotConfirmedException':
            return create_response(400, {
                'success': False,
                'message': 'User is not confirmed'
            })

        return create_response(400, {
            'success': False,
            'message': error_message
        })

    except Exception as e:
        print(f"Unexpected error: {str(e)}")
        print(f"Error type: {type(e)}")
        print(f"Traceback: {__import__('traceback').format_exc()}")
        return create_response(500, {
            'success': False,
            'message': f'Internal server error during signin: {str(e)}'
        })

def lambda_handler(event, context):
    try:
        print(f"Received event: {json.dumps(event)}")

        username = event.get('username')
        password = event.get('password')

        return handle_signin(username, password)

    except Exception as e:
        print(f"Lambda handler error: {str(e)}")
        return create_response(500, {
            'success': False,
            'message': f'Internal server error: {str(e)}'
        })
