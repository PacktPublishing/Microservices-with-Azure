# Microservices with Azure
This is the code repository for [Microservices with Azure](https://www.packtpub.com/virtualization-and-cloud/microservices-azure?utm_source=github&utm_medium=repository&utm_content=9781787121140), published by [Packt](https://www.packtpub.com/). It contains all the supporting project files necessary to work through the book from start to finish.

## About the Book
Microsoft Azure is rapidly evolving and is widely used as a platform on which you can build Microservices that can be deployed on-premise and on-cloud heterogeneous environments through Microsoft Azure Service Fabric. This book will help you understand the concepts of Microservice application architecture and build highly maintainable and scalable enterprise-grade applications using the various services in Microsoft Azure Service Fabric. We will begin by understanding the intricacies of the Microservices architecture and its advantages over the monolithic architecture and Service Oriented Architecture (SOA) principles. We will present various scenarios where Microservices should be used and walk you through the architectures of Microservice-based applications. Next, you will take an in-depth look at Microsoft Azure Service Fabric, which is the best–in-class platform for building Microservices. You will explore how to develop and deploy sample applications on Microsoft Azure Service Fabric to gain a thorough understanding of it.

Building Microservice-based application is complicated. Therefore, we will take you through several design patterns that solve the various challenges associated with realizing the Microservices architecture in enterprise applications. Each pattern will be clearly illustrated with examples that you can keep referring to when designing applications.

Finally, you will be introduced to advanced topics such as Serverless computing and DevOps using Service Fabric, to help you undertake your next venture with confidence.

## More on Companion Website
The companion website has further content to enrich your knowledge. Therefore, consider bookmarking the companion website to stay in touch: https://microserviceswithazure.com

## Instructions and Navigations
All of the code is organized into folders. Each folder starts with a number followed by the application name. For example, Chapter04

The code will look like the following:
       
       static int ngpios;
       static int gpios[2] = { -1 , -1 };
       module_param_array(gpios, int, &ngpios, S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP);
       MODULE_PARM_DESC(gpios, "Defines the GPIOs number to be used as a list of"
                        " numbers separated by commas.");

       /* Logging stuff */
       #define __message(level, fmt, args...)                                  \
                       printk(level "%s: " fmt "\n" , NAME , ## args)

There are no code files for the chapters 01, 02, 03, 04, 07, 10, 11, 12.


### Software and hardware requirements:

The examples found in this book require a Microsoft Azure subscription. You can sign up
for a free trial account via the Azure website: https://azure.microsoft.com/.

You will need Windows 7+, latest Service Fabric SDK, latest Azure SDK, latest Azure
PowerShell, 4GB RAM, 30 GB available Hard Disk space, Visual Studio 2017, and Visual
Studio Team Service for executing the practical examples in this book.



## Related Products:

* [C# 6 and .NET Core 1.0: Modern Cross-Platform Development]( https://www.packtpub.com/application-development/c-6-and-net-core-10?utm_source=github&utm_medium=repository&utm_content=9781785285691 )

* [Implementing DevOps with Microsoft Azure]( https://www.packtpub.com/networking-and-servers/implementing-devops-microsoft-azure?utm_source=github&utm_medium=repository&utm_content=9781787127029 )

* [Robust Cloud Integration with Azure]( https://www.packtpub.com/virtualization-and-cloud/robust-cloud-integration-azure?utm_source=github&utm_medium=repository&utm_content=9781786465573 )

* [Mastering Identity and Access Management with Microsoft Azure]( https://www.packtpub.com/virtualization-and-cloud/mastering-identity-and-access-management-microsoft-azure?utm_source=github&utm_medium=repository&utm_content=9781785889448 )


### Download a free PDF

 <i>If you have already purchased a print or Kindle version of this book, you can get a DRM-free PDF version at no cost.<br>Simply click on the link to claim your free PDF.</i>
<p align="center"> <a href="https://packt.link/free-ebook/9781787121140">https://packt.link/free-ebook/9781787121140 </a> </p>