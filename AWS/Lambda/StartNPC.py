import json
import boto3
import os
import logging

logger = logging.getLogger()
logger.setLevel(logging.INFO)

def create_context_prompt(player_stats):
    """
    Creates a context-aware prompt for the starting point NPC based on player statistics
    """
    context = f"""You are an NPC character at the starting point of a survival game. You talk directly to the player.

Your task: Respond with a single sentence that is between 8-15 words, teasing, without any extra explanations or meta-text.
Your response must reference the player's stats or game history when relevant.

Game Elements:

Player has completed {player_stats['totalGamesPlayed']} total games.(if 0, it's the first game)
Current streak: {player_stats['consecutiveGamesPlayed']}.
Personal best: {player_stats['daysSurvivedHighScore']} days.
Last defeated by: {player_stats['killedLastGameBy']}.
If Player has completed 0 total games, treat him as a new player at his first game.

Game information:
In the game the player builds, farms, and fights.

Response Guidelines: 

Use humor and a lot of teasing.
Keep interactions varied and creative to feel fresh and dynamic.
Do not exceed 15 words in your response.

Examples:

"Those skeletons are tricky! Try building more defense towers this time!"
"Nine days? Not bad! Let's aim for ten this round!"
"The demon killed you? That's fair."
"Let's see if you can get past day one this time!"
"Three games in a row? Now that's dedication!"
"That was only a slime! How did you die?"
"Looks like you came to reclaim your pride."
Remember, Your response must be a single sentence, directly addressing the player. Do not explain or add unnecessary text.
It is very important that the text will not be longer than 15 words.

Examples for what you should not say:
"Only zero Days? Let's aim higher!" - you should not state the streak at all if it's zero."""

    return context

def lambda_handler(event, context):
    """
    Lambda function for Starting Point NPC using Amazon Nova Lite v1:0 via Bedrock
    Generates personalized responses based on player statistics
    """
    logger.info(f"Received event: {json.dumps(event)}")

    # Initialize Bedrock client
    bedrock = boto3.client('bedrock-runtime')

    try:
        # Extract player stats
        if 'body' in event:
            try:
                player_stats = json.loads(event['body']) if isinstance(event['body'], str) else event['body']
                logger.info(f"Parsed player stats: {json.dumps(player_stats)}")
            except json.JSONDecodeError as e:
                logger.error(f"Failed to parse event body: {str(e)}")
                return {
                    'statusCode': 400,
                    'body': json.dumps({
                        'error': f"Invalid JSON format in request body: {str(e)}"
                    })
                }
        else:
            player_stats = event
            logger.info(f"Using event directly as player stats: {json.dumps(player_stats)}")

        # Validate required player stats structure
        required_fields = ['totalGamesPlayed', 'consecutiveGamesPlayed', 
                           'killedLastGameBy', 'daysSurvivedLastGame', 
                           'daysSurvivedHighScore']

        missing_fields = [field for field in required_fields if field not in player_stats]
        if missing_fields:
            error_msg = f"Missing required fields: {', '.join(missing_fields)}"
            logger.error(error_msg)
            return {
                'statusCode': 400,
                'body': json.dumps({
                    'error': error_msg
                })
            }

        # Update killedLastGameBy if consecutiveGamesPlayed is 0
        if player_stats['consecutiveGamesPlayed'] == 0:
            player_stats['killedLastGameBy'] = ''

    except Exception as e:
        logger.error(f"Error processing input: {str(e)}")
        return {
            'statusCode': 500,
            'body': json.dumps({
                'error': f"Error processing input: {str(e)}"
            })
        }

    # Generate context-aware system prompt
    system_prompt = create_context_prompt(player_stats)
    logger.info("Generated system prompt")

    # Prepare the Bedrock request payload with the exact Nova schema
    payload = {
        "system": [
            {
                "text": system_prompt
            }
        ],
        "messages": [
            {
                "role": "user",
                "content": [
                    {
                        "text": "Generate a greeting for the player."
                    }
                ]
            }
        ],
        "inferenceConfig": {
            "temperature": float(os.environ.get('TEMPERATURE', 0.7)),
            "max_new_tokens": int(os.environ.get('MAX_NEW_TOKENS', 100)),
            "stopSequences": ["\n"]
        }
    }

    try:
        logger.info(f"Calling Bedrock with payload: {json.dumps(payload)}")
        # Call Bedrock with the model ID from the environment
        response = bedrock.invoke_model(
            modelId=os.environ['MODEL_ID'],
            body=json.dumps(payload)
        )

        # Add detailed logging of raw response
        raw_response = response['body'].read()
        logger.info(f"Raw Bedrock response: {raw_response}")

        try:
            response_body = json.loads(raw_response)
            logger.info(f"Parsed Bedrock response: {json.dumps(response_body)}")

            # Extract the text response from the correct path in the response structure
            try:
                npc_response = response_body['output']['message']['content'][0]['text']
                # Remove any extra quotes from the response
                npc_response = npc_response.strip('"')
                logger.info(f"Extracted NPC response: {npc_response}")

                # Return the successful response
                return {'response': npc_response}

            except (KeyError, IndexError) as e:
                logger.error(f"Failed to extract response from structure: {str(e)}")
                return {
                    'statusCode': 500,
                    'body': json.dumps({
                        'error': 'Failed to extract response from Bedrock',
                        'raw_response': response_body
                    })
                }

        except json.JSONDecodeError as e:
            logger.error(f"Failed to parse Bedrock response: {str(e)}")
            return {
                'statusCode': 500,
                'body': json.dumps({
                    'error': f"Invalid response format from Bedrock: {str(e)}",
                    'raw_response': raw_response.decode('utf-8')
                })
            }

    except Exception as e:
        logger.error(f"Error calling Bedrock: {str(e)}")
        return {
            'statusCode': 500,
            'body': json.dumps({
                'error': f"Error generating NPC response: {str(e)}"
            })
        }
