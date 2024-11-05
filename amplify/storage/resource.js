import { defineBackend } from '@aws-amplify/backend';
import { auth } from './auth/resource.ts';
import { data } from './data/resource.ts';
const backend = defineBackend({
    auth,
    data,
});
backend.addOutput({
    storage: {
        aws_region: "eu-north-1",
        bucket_name: "fbf-assets-s3",
    },
});
