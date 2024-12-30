import json
import boto3
import os
import hmac
import base64
import hashlib
from botocore.exceptions import ClientError

# Initialize clients
cognito = boto3.client('cognito-idp')
dynamodb = boto3.resource('dynamodb')
users_table = dynamodb.Table(os.environ['USERS_TABLE_NAME'])

# Cognito constants
USER_POOL_ID = os.environ['USER_POOL_ID']
CLIENT_ID = os.environ['CLIENT_ID']
CLIENT_SECRET = os.environ['CLIENT_SECRET']

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

def check_username_exists(username):
    try:
        response = users_table.get_item(
            Key={'username': username}
        )
        return 'Item' in response
    except Exception as e:
        print(f"Error checking username: {str(e)}")
        return False

def save_user_email(username, email):
    try:
        users_table.put_item(
            Item={
                'username': username,
                'email': email
            }
        )
        return True
    except Exception as e:
        print(f"Error saving user email: {str(e)}")
        return False

def handle_signup(email, password, username):
    try:
        print(f"Starting signup process for email: {email}")

        if not email or not password or not username:
            print("Missing required fields")
            return create_response(400, {
                'success': False,
                'message': 'Email, password and username are required'
            })

        # Check if username exists
        if check_username_exists(username):
            return create_response(400, {
                'success': False,
                'message': 'Username already exists'
            })

        # Generate the secret hash
        secret_hash = get_secret_hash(email)

        print("Calling Cognito sign_up")
        response = cognito.sign_up(
            ClientId=CLIENT_ID,
            Username=email,
            Password=password,
            SecretHash=secret_hash,
            UserAttributes=[
                {
                    'Name': 'email',
                    'Value': email
                }
            ]
        )

        # Save username-email mapping
        if not save_user_email(username, email):
            return create_response(500, {
                'success': False,
                'message': 'Error saving user data'
            })

        return create_response(200, {
            'success': True,
            'message': 'User registration initiated successfully',
            'userSub': response['UserSub']
        })

    except ClientError as e:
        error_code = e.response['Error']['Code']
        error_message = e.response['Error']['Message']
        print(f"Cognito error: {error_code} - {error_message}")

        if error_code == 'UsernameExistsException':
            return create_response(400, {
                'success': False,
                'message': 'An account with this email already exists'
            })
        elif error_code == 'InvalidPasswordException':
            return create_response(400, {
                'success': False,
                'message': 'Password does not meet requirements'
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
            'message': f'Internal server error during signup: {str(e)}'
        })

def handle_verify(body):
    try:
        print(f"Processing verification with body: {json.dumps(body)}")

        email = body.get('email')
        code = body.get('code')

        if not email or not code:
            return create_response(400, {
                'success': False,
                'message': 'Email and verification code are required'
            })

        secret_hash = get_secret_hash(email)

        print(f"Attempting to verify user with email: {email}")
        cognito.confirm_sign_up(
            ClientId=CLIENT_ID,
            Username=email,
            ConfirmationCode=code,
            SecretHash=secret_hash
        )

        return create_response(200, {
            'success': True,
            'message': 'User verified successfully'
        })

    except ClientError as e:
        error_code = e.response['Error']['Code']
        print(f"ClientError in verify: {error_code} - {e.response['Error']['Message']}")

        if error_code == 'CodeMismatchException':
            return create_response(400, {
                'success': False,
                'message': 'Invalid verification code'
            })
        elif error_code == 'ExpiredCodeException':
            return create_response(400, {
                'success': False,
                'message': 'Verification code has expired'
            })

        return create_response(400, {
            'success': False,
            'message': e.response['Error']['Message']
        })

    except Exception as e:
        print(f"Unexpected error in verify: {str(e)}")
        print(f"Error type: {type(e)}")
        print(f"Error traceback: {__import__('traceback').format_exc()}")
        return create_response(500, {
            'success': False,
            'message': f'Internal server error during verification: {str(e)}'
        })

def lambda_handler(event, context):
    try:
        print(f"Received event: {json.dumps(event)}")

        body = event
        print(f"Request body: {json.dumps(body)}")

        email = body.get('email')
        password = body.get('password')
        username = body.get('username')
        code = body.get('code')

        print(f"Extracted email: {email}")
        print("Password/code received: [REDACTED]")

        if code:
            return handle_verify(body)
        else:
            return handle_signup(email, password, username)

    except Exception as e:
        print(f"Lambda handler error: {str(e)}")
        print(f"Error type: {type(e)}")
        print(f"Traceback: {__import__('traceback').format_exc()}")
        return create_response(500, {
            'success': False,
            'message': f'Internal server error: {str(e)}'
        })
