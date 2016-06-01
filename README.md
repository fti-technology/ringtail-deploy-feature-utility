# ringtail-deploy-feature-utility
Utility program to retrieve and write keys to the Ringtail database to control end user exposure of features.
Utilized by the Ringtail deployment suite.

## Purpose

Reconciles a version of Ringtail with which feature keys can be installed on it.
Can insert keys provided to it into a Ringtail database.

## Usage

### Getting Keys

    ringtail-deploy-feature-utility --getkeys --filter="PREVIEW"
    ringtail-deploy-feature-utlility /getfeaturekeys --portalconnection="your connection string"

### Setting Keys

    // keyExmple: [{"FeatureKey":"KEY3","Description":"Some exmaple key","MinorKey":"8.6.1002"}]
    // keys are base64 encoded JSON
    ringtail-deploy-feature-utility --bulkdatapath=="D:\path" --keys="your keys" /base64
