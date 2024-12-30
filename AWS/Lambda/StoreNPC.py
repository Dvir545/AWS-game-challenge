import json
import boto3
import os
import logging

# Set up logging
logger = logging.getLogger()
logger.setLevel(logging.INFO)

def create_context_prompt(game_state):
    """
    Creates a context-aware prompt for the store advisor NPC based on game state
    """
    # Get all data from the correct nested structure - using camelCase to match Unity's JSON
    player_status = game_state.get('playerStatus', {})
    inventory = player_status.get('inventory', {})
    world_status = player_status.get('worldStatus', {})
    shops = world_status.get('shops', {})
    last_round = player_status.get('lastRoundActivity', {})
    next_enemies = player_status.get('nextRoundEnemies', {})
    previous_responses = player_status.get('previousResponses', [])
    
    # Format inventories
    crops = inventory.get('crops', {})
    crops_owned = ', '.join([f"{crop}: {count}" for crop, count in crops.items()])
    
    materials = inventory.get('towerMaterials', {})
    materials_owned = ', '.join([f"{mat}: {count}" for mat, count in materials.items()])
    
    # Format shop inventories
    seeds = shops.get('seedShopAvailableSeeds', {})
    available_seeds = ', '.join([f"{seed}: {count}" for seed, count in seeds.items()])
    
    shop_materials = shops.get('resourcesShopAvailableMaterials', {})
    available_materials = ', '.join([f"{mat}: {count}" for mat, count in shop_materials.items()])
    
    tools = shops.get('toolShopAvailableUpgradeLevel', {})
    tool_upgrades = ', '.join([f"{tool} to lvl{level + 1}" for tool, level in tools.items()])

    utilities = shops.get('utilityShopAvailableUpgradeLevels', {})
    utility_upgrades = ', '.join([f"{util} to lvl{level}" for util, level in utilities.items()])
    
    # Format enemies that have count > 0
    next_round_threats = ', '.join([f"{enemy}({count})" for enemy, count in next_enemies.items() if count > 0])
    
    # Get last response if available
    last_response = previous_responses[-1] if previous_responses else "No previous response"

    context = f"""You are a wise merchant NPC who gives strategic advice to players who are warriors, and survivals.
        IMPORTANT: You must respond with EXACTLY ONE SHORT SENTENCE (8-15 words).
        Your advice should be direct and concise.
        
        VERY IMPORTANT:give advice that is different from: "{last_response}". do not suggest similar things to this!

        Player Status:
        Health: {player_status.get('health', 0)}/12
        Money: {player_status.get('money', 0)} gold
        Owned crops: {crops_owned}
        Owned materials: {materials_owned}

        World Status:
        Day {world_status.get('daysSurvived', 1)}
        {world_status.get('existingTowers', 0)} existing towers

        Shops Inventory:
        Seeds Available:
        {available_seeds}

        Materials Available:
        {available_materials}

        Tool Upgrades Available:
        {tool_upgrades}

        Utility Upgrades Available:
        {utility_upgrades}

        Last Round Activity:
        Took {last_round.get('damageTaken', 0)} damage
        Planted {last_round.get('cropsPlanted', 0)} crops
        Harvested {last_round.get('cropsHarvested', 0)} crops
        Lost {last_round.get('cropsDestroyed', 0)} crops
        Built {last_round.get('towersBuilt', 0)} towers
        Lost {last_round.get('towersDestroyed', 0)} towers

        Next Round Threats:
        {next_round_threats}

        Strategic Guidelines:
        Prioritize needs based on ALL available information(not in specific order):
        - Economic status (crops and money) - if money is low (under 100)
        - Defensive needs (towers and materials)
        - Upcoming threats
        - Available upgrades
        - health
        
        Key Considerations:
        - If many chickens are coming (more than 5), prioritize crop protection 
        - Only if health is low (below 3), suggest upgrading health(health bar), or careful play
        - If crops were destroyed last round, suggest to harvest before sunset
        - If money is high, suggest worthwhile purchases or upgrades (player upgrades/materials for building/tools upgrades). don't mention how much money the player has.
        - If multiple threats exist, prioritize the most dangerous ones

        Monster Alert Thresholds:
        - Chickens: Alert if more than 3 (crop threat)
        - Orcs: Alert if more than 2 (structure threat)
        - Demon Boss: Alert if any present (major threat)

        resources:
        Crops - income (the player can't eat in the game - it's only for income, the higher crop the better): wheat, carrot, tomato, corn, pumpkin
        Materials - attack and defense towers (and their costs. the higher material the better): wood(20), stone(50), iron(100), gold(250), diamond(500)
        Tools - max lvl is 3 (don't suggest upgrading if the lvl is 3!): sword(for fighting), hammer(for building), hoe(for farming)

        IMPORTANT RULES:
        1. Give ONE SENTENCE between 8-15 words
        2. End with an exclamation mark
        3. Use a fun and light tone
        4. Don't repeat previous advice
        5. Focus on the most urgent current need"""

    return context

def lambda_handler(event, context):
    """
    Lambda function for Store NPC using Amazon Nova lite v1:0 via Bedrock
    """
    logger.info(f"Received event: {json.dumps(event)}")
    
    # Read environment variables
    bedrock_model_id = os.getenv('BEDROCK_MODEL_ID', 'amazon.nova-lite-v1:0')
    
    # Initialize Bedrock client
    bedrock = boto3.client('bedrock-runtime')
    
    try:
        # Generate context-aware system prompt
        system_prompt = create_context_prompt(event)
        logger.info("Generated system prompt")
        
        # Prepare the Bedrock request payload with Nova schema
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
                            "text": "Give one short sentence of advice to the player."
                        }
                    ]
                }
            ],
            "inferenceConfig": {
                "temperature": 0.7,
                "max_new_tokens": 100,
                "stopSequences": ["\n"]
            }
        }

        logger.info(f"Calling Bedrock with payload: {json.dumps(payload)}")
        # Call Bedrock with Amazon Nova Micro v1:0 model
        response = bedrock.invoke_model(
            modelId=bedrock_model_id,
            body=json.dumps(payload)
        )
        
        # Add detailed logging of raw response
        raw_response = response['body'].read()
        logger.info(f"Raw Bedrock response: {raw_response}")
        
        try:
            response_body = json.loads(raw_response)
            logger.info(f"Parsed Bedrock response: {json.dumps(response_body)}")
            
            # Extract the text response from Nova's response structure
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
        logger.error(f"Error in lambda_handler: {str(e)}")
        return {
            'statusCode': 500,
            'body': json.dumps({
                'error': f"Error generating NPC response: {str(e)}"
            })
        }
