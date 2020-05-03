![Upload Website](https://github.com/t-j-c/tay-co/workflows/Upload%20Website/badge.svg?branch=master)

This repository contains the code for my personal website at http://tay-co.com/

The code is structured in the following directories:
- `blogs`
  - Contains: 
    - The actual contents of the blogs (written in markdown files)
    - An `index.json` file listing metadata for each blog
  - When changes in this directory are pushed to the master branch, the contents are uploaded to an AWS S3 bucket
- `src`
  - Contains all of the source code
  - When changes in this directory are pushed to the master branch, the contents are built, published, and uploaded to an AWS S3 bucket
- `test`
  - Contains relevant tests
- `.github/workflows`
  - Contains `.yml` files defining the different workflows triggered by [Github Actions](https://help.github.com/en/actions/reference/workflow-syntax-for-github-actions)

## Contributing

Contributions to the repository are more than welcome. These may include:
- Fixes to typos throughout the site (especially in blogs)
- Bug fixes
- Style updates
- Optimizations

The steps for contributing are as follows:
1. Create a branch off of the latest `master`
2. Submit a pull request from your branch containing changes into `master`
3. The PR will trigger a workflow to build and test your changes. Once this succeeds and required approvals are made, the PR can be completed.
4. Upon PR completion, necessary actions will be automatically triggered to deploy relevant changes.
