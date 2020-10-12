# ringtail-deploy-feature-utility
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Ffti-technology%2Fringtail-deploy-feature-utility.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Ffti-technology%2Fringtail-deploy-feature-utility?ref=badge_shield)

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

    ringtail-deploy-feature-utility --bulkdatapath="C:\pathToSqlComponents" --keysfile="your keys.json" /base64


## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Ffti-technology%2Fringtail-deploy-feature-utility.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2Ffti-technology%2Fringtail-deploy-feature-utility?ref=badge_large)