import boto3

BUCKET = "bird-sounds"

client = boto3.client(
    's3',
    endpoint_url="http://localhost:9000",
    aws_access_key_id="admin",
    aws_secret_access_key="Pa55w.rd",
)
