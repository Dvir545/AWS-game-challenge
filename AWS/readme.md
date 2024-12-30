# AWS

This repository contains the AWS infrastructure and backend services for our Unity WebGL game. The project uses AWS CDK for infrastructure deployment and AWS Lambda functions for various game services.

## Project Structure

# ****image******

## Overview

This project implements the backend infrastructure for a Unity WebGL game using AWS services. After building the game in Unity with WebGL build settings, we deploy it using AWS CDK, which uploads the game and creates other necessary AWS resources.

## Lambda Functions
#### all lambdas are invoked with ApiGW.
The project includes several Python-based Lambda functions that handle different aspects of the game:

### Player Authentication and Management
- `RegistrationCognito.py`: Handles new player registration through AWS Cognito
- `SignInCognito.py`: Manages player sign-in through AWS Cognito

### Game Progress and Data
- `LoadSaveLambda.py`: Retrieves existing save game data for registered players from DynamoDB
- `SaveGameData.py`: Creates and updates save game data for registered players in DynamoDB
- `GetLeaderBoard.py`: Fetches the current leaderboard from DynamoDB
- `NewRecord.py`: Creates new score records in DynamoDB when players achieve high scores

### NPC Interactions
- `StartNPC.py`: Handles responses for the starting NPC interactions
- `StoreNPC.py`: Manages Middle NPC responses and interactions


## Infrastructure Components
### Website Hosting

- S3 Bucket: Hosts the Unity WebGL build files

## Content Delivery

### CloudFront Distribution

- Custom domain support (farmbuildfight.click)


## Domain Management

### Route 53

- Custom domain configuration
- A-record pointing to CloudFront distribution
- Integrated with existing hosted zone
- ACM Certificate (integrated with cloudfront)


## API Integration

The Lambda functions are exposed through API Gateway, creating RESTful endpoints that the Unity WebGL build can interact with. These endpoints handle:
- Player authentication
- Game state management
- Leaderboard operations
- NPC interactions

Each Lambda function is designed to handle specific game functionality while maintaining security through Cognito authentication and proper IAM roles.
