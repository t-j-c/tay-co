*This is part of a series of posts on setting up a static website with Blazor and AWS.<br>
In Part 1 of this series, we go over what's needed to build a static website with Blazor WebAssembly.<br>
In Part 2, we look at how you can host that website in AWS.<br>
In Part 3, we setup a CI/CD pipeline using GitHub Actions.*

---

## What's needed

One of the benefits of developing software using cloud services is the ease of use.
The major providers (namely Microsoft, Amazon, and Google) all give a wide array of offerings, allowing robust and complex solutions.

Since our site is going to be static, it should be cheap and simple to maintain.
We don't need any sort of server-side compute, we just need a place to host our files and provide public access to them.
AWS has great documentation on [hosting a static website](https://aws.amazon.com/getting-started/hands-on/host-static-website/), so I mainly followed that to set everything up.
Here's an overview of the services that will be needed:
- [Simple Storage Service (aka S3)](https://aws.amazon.com/s3/?nc=sn&loc=0)
  - As the name suggests, this is where the files for our website will be stored
- [Route 53](https://aws.amazon.com/route53/)
  - This allows us to associate a public domain name with our content in S3
- [CloudFront](https://aws.amazon.com/cloudfront/?nc2=type_a)
  - This one is optional. It simply provides better performance when loading our website by caching data in locations closer to end-users

This diagram from the AWS documentation gives a great outline of how these services interact:
![](https://d1.awsstatic.com/Projects/v1/AWS_StaticWebsiteHosting_Architecture_4b.da7f28eb4f76da574c98a8b2898af8f5d3150e48.png)

---

## Pricing

Aside from the low maintenance and ease of use, another benefit to be gained from this approach is the low cost.
I ended up using purchasing a domain with Route 53, which has been the most expensive part of the website at around $12.

The billing model of these services are base off of your usage. So there's no large upfront fees or subscription costs.
For most cases, static sites will cost somewhere between pennies to a few dollars per month.

---

## Publishing the Blazor app

As mentioned earlier, the AWS documentation for hosting a static site is pretty thorough, so I'd recommend following along with that.
The whole process is easy to follow and took me less than 30 minutes.

The only part that I'll give some guidance to is the portion of uploading the actual website content.
Once you have the S3 bucket for you root domain created, you can then add the contents of your Blazor app.

To do so, you'll first need to create the publishable contents of your app using one of the following methods:
 - [Visual Studio](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/blazor/?view=aspnetcore-3.1&tabs=visual-studio#publish-the-app)
   - When presented with the option to pick a publish target, choose "Folder" 
 - [dotnet command line](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish)
   - Here is the command that I ran to publish my app: `dotnet publish -c Release -o publish Tayco.sln`
   - I'd recommend getting familiarized with the CLI if you're not already. We'll be using this in a later post in setting up a Continuous Deployment pipeline.

Once your app has been published, you simply need to upload the contents into the S3 root domain bucket.
Whichever method above you choose to publish with, you should ultimately end up with a directory containing your `index.html`.
This is the directory that you'll want to copy into your S3 bucket.
This folder will contain everything in your `wwwroot` folder, along with the necessary DLLs and Blazor files.

As mentioned in Part 1, one important thing to consider here is that anything your copy to this S3 bucket will be publicly accessible.
If this is a concern for you, then this solution is probably not appropriate.
You may want to consider a solution where the private contents can be contained on a private server.

---

## Summary

With the Blazor app published to your S3 bucket, along with all of the other AWS components set up, you should now be able to access your site!

Now, there's a good chance that you'll want to continue making changes to your website.
And since you probably don't want to have to keep manually uploading the contents to AWS for each change, one thing we can do is set up a CI/CD pipeline.
I'll be doing a follow up to this post as a third part in this series where I'll go over the steps in setting up that pipeline.

Stay tuned!