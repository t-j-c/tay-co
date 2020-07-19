*This is part of a series of posts on setting up a static website with Blazor and AWS.<br>
In Part 1 of this series, we go over what's needed to build a static website with Blazor WebAssembly.<br>
In Part 2, we look at how you can host that website in AWS.<br>
In Part 3, we setup a CI/CD pipeline using GitHub Actions.*

---

As a result of Part 2 in this series, we succesfully published a Blazor WebAssembly app as a static website in AWS. 
This process essentially boiled down to the following:
  
1. Build our Blazor app
2. Grab the distributable contents
3. Copy them to our S3 bucket in AWS

Now our website is live which is great. However, we're probably going to be making changes to the website over time - as with almost any piece of software.
We could just repeat the process we've followed above, manually building and copying the contents into production every time we make a change.
Since I'll likely be the sole contributor to my website and changes will be relatively infrequent, this would probably be manageable (even though I'd cringe every time).
There are a few factors in software delivery that when increased make this process quickly unmanageable:

1. Frequency of changes
2. Number of contributors

This problem is generally solved under the umbrella of [Continuous Integration and Continuous Deployment](https://www.redhat.com/en/topics/devops/what-is-ci-cd) (i.e. CI/CD),
which entails automating the build/deployment steps that we would otherwise do manually.
There is an ever growing list of tools that can be used to help implement CI/CD.
We'll be using [GitHub Actions](https://docs.github.com/en/actions/getting-started-with-github-actions/about-github-actions) to automate the process of building our Blazor WASM app and deploying it to AWS.

![](https://i.ibb.co/4f5CDN5/blazor-actions-aws.png)

---

# Building Workflows

I decided to go with GitHub Actions in this project for a few reasons:

- [x] I already had my source code in a GitHub repository.
- [x] There is a free tier available that meets my needs (see [additional pricing options](https://github.com/pricing)).
- [x] I found the [documentation](https://docs.github.com/en/actions/configuring-and-managing-workflows) thorough and easy to navigate.
  
As mentioned previously, we have a few manual steps that we can follow to get our website into production.
To translate that into GitHub Actions, we'll need to create a [workflow](https://docs.github.com/en/actions/configuring-and-managing-workflows/configuring-a-workflow).
The idea of a workflow is pretty standard across different CI/CD implementations. At it's core, it's just a definition of your build/deployment process.
That definition is generally stored in a YAML file. 

With GitHub Actions, to create a workflow you simply add the definition as a `.yml` file in the `/.github/workflows` directory in your repository.
Below is the current workflow definition for uploading my website (don't worry, we'll break down the pieces of this next):

```
name: Upload Website

on:
  workflow_dispatch:
    inputs:
      input_name:
        required: false
        default: "Upload Website - Manual Trigger"
  push:
    branches:
      - master
    paths:
      - 'src/**'

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@master
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '3.1.300'
      - run: dotnet build -c Release
      - run: dotnet test -c Release --no-build
      - run: dotnet publish -c Release --no-build -o publish Tayco.sln
      - uses: actions/upload-artifact@v1
        with:
          name: dist
          path: publish/wwwroot
  deploy:
    needs: [build]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/download-artifact@v1
        with:
          name: dist
      - uses: jakejarvis/s3-sync-action@master
        with:
          args: --acl public-read --follow-symlinks --delete
        env:
          AWS_S3_BUCKET: ${{ secrets.AWS_S3_BUCKET }}
          AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
          AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          SOURCE_DIR: 'dist/'
```

As you can see, there are a number of grouped sections in this file. Let's break it down to understand what's going on.

## [`on`](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions#on)

```
on:
  workflow_dispatch:
    inputs:
      input_name:
        required: false
        default: "Upload Website - Manual Trigger"
  push:
    branches:
      - master
    paths:
      - 'src/**'
```

The `on` keyword is used to define an event that the workflow will be triggered by.
In our case, this workflow can be triggered by two different events:

1. [`workflow_dispatch`](https://github.blog/changelog/2020-07-06-github-actions-manual-triggers-with-workflow_dispatch/)
    1. This is a recent addition that allows the workflow to be manually queued through the UI. Prior to this you would have to trigger a different event (e.g. push a commit) in order to run the workflow.
2. `push`
    1. As you might guess, this event is any push to the repository given a set of conditions. 
    2. Our conditions are any push to the `master` branch with changes in the `src/` directory.
    
## [`jobs`](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions#jobs)

The `jobs` keyword defines what the workflow will actually do in response the events defined above.
Each job generally specifies:
  1. [`runs-on`](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions#jobsjob_idruns-on): The environment that the job will run in.
  2. [`steps`](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions#jobsjob_idsteps): These are the tasks that will be run within the job.

It's also important to note that jobs will run in parallel by default.
So if you have dependencies, you'll need to explicitly declare those using [`needs](https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions#jobsjob_idneeds).

Our workflow consists of two rather straightforward jobs: `build` and `deploy`.

### `build`

```
build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@master
      - uses: actions/setup-dotnet@v1
        with:
          dotnet-version: '3.1.300'
      - run: dotnet build -c Release
      - run: dotnet test -c Release --no-build
      - run: dotnet publish -c Release --no-build -o publish Tayco.sln
      - uses: actions/upload-artifact@v1
        with:
          name: dist
          path: publish/wwwroot
```

