from constructs import Construct
from aws_cdk import (
    Stack,
    aws_s3 as s3,
    aws_s3_deployment as s3deploy,
    aws_cloudfront as cloudfront,
    aws_cloudfront_origins as origins,
    aws_route53 as route53,
    aws_route53_targets as targets,
    aws_certificatemanager as acm,
    CfnOutput,
    RemovalPolicy
)
import os

class UnityWebGLStack(Stack):
    def __init__(self, scope: Construct, construct_id: str, **kwargs) -> None:
        super().__init__(scope, construct_id, **kwargs)

        # Create S3 bucket for website hosting
        website_bucket = s3.Bucket(
            self,
            "UnityWebGLBucket",
            bucket_name="unity-webgl-bucket",
            website_index_document="index.html",
            website_error_document="index.html",
            public_read_access=True,
            block_public_access=s3.BlockPublicAccess.BLOCK_ACLS,
            removal_policy=RemovalPolicy.DESTROY,
            auto_delete_objects=True,
            cors=[s3.CorsRule(
                allowed_methods=[s3.HttpMethods.GET],
                allowed_origins=['*'],
                allowed_headers=['*'],
                max_age=3000
            )]
        )

        # Import existing ACM certificate
        certificate = acm.Certificate.from_certificate_arn(
            self,
            "Certificate",
            certificate_arn=f"arn:aws:acm:us-east-1:{Stack.of(self).account}:certificate/<CERTIFICATE_ID>"
        )

        # Create CloudFront distribution
        distribution = cloudfront.Distribution(
            self,
            "UnityWebGLDistribution",
            default_behavior=cloudfront.BehaviorOptions(
                origin=origins.S3Origin(website_bucket),
                viewer_protocol_policy=cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                cache_policy=cloudfront.CachePolicy.CACHING_OPTIMIZED,
                origin_request_policy=cloudfront.OriginRequestPolicy.CORS_S3_ORIGIN,
                compress=True
            ),
            domain_names=["example.com"],
            certificate=certificate,
            default_root_object="index.html",
            price_class=cloudfront.PriceClass.PRICE_CLASS_100,
            enable_logging=True,
        )

        # Import existing Route 53 hosted zone
        hosted_zone = route53.HostedZone.from_hosted_zone_attributes(
            self,
            "HostedZone",
            hosted_zone_id="<HOSTED_ZONE_ID>",
            zone_name="farmbiuldfight.click"
        )

        # Create Route 53 A record
        route53.ARecord(
            self,
            "AliasRecord",
            zone=hosted_zone,
            target=route53.RecordTarget.from_alias(
                targets.CloudFrontTarget(distribution)
            ),
            record_name="farmbiuldfight.click"  
        )

        # Deploy Brotli compressed files
        s3deploy.BucketDeployment(
            self,
            "UnityWebGLDeploymentBrotli",
            sources=[s3deploy.Source.asset(r"/path/to/build/files")],
            destination_bucket=website_bucket,
            distribution=distribution,
            retain_on_delete=False,
            include=["*.br"],
            exclude=["*"],
            content_encoding="br",
            prune=False,
            memory_limit=1024
        )

        # Deploy other files
        s3deploy.BucketDeployment(
            self,
            "UnityWebGLDeploymentOther",
            sources=[s3deploy.Source.asset(r"/path/to/build/files")],
            destination_bucket=website_bucket,
            distribution=distribution,
            retain_on_delete=False,
            exclude=["*.br"],
            prune=False,
            memory_limit=1024
        )

        # Outputs
        CfnOutput(
            self,
            "BucketName",
            value=website_bucket.bucket_name,
            description="Name of the S3 bucket"
        )

        CfnOutput(
            self,
            "DistributionId",
            value=distribution.distribution_id,
            description="ID of the CloudFront distribution"
        )

        CfnOutput(
            self,
            "CloudFrontURL",
            value=f"https://{distribution.distribution_domain_name}",
            description="URL of the CloudFront distribution"
        )

        CfnOutput(
            self,
            "DomainName",
            value="https://farmbiuldfight.click",
            description="Website domain name"
        )
