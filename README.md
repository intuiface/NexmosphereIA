# Nexmosphere Interface Asset for Intuiface

This project contains a Nexmosphere Interface Asset for Intuiface Player & Composer.

The [Nexmosphere](https://nexmosphere.com) Interface Asset enables you to communicate with Nexmosphere devices from within an Intuiface experience.
This Interface Asset was developed to communicate with all Nexmosphere controllers:
* [XN RANGE | NANO CONTROLLERS](https://nexmosphere.com/product-category/xn-nano/)
* [XC RANGE | COMPACT CONTROLLERS](https://nexmosphere.com/product-category/xc-compact-controllers/)
* [XM RANGE | MODULAR CONTROLLERS](https://nexmosphere.com/product-category/xm-range/)

Meaning it handles all of the following elements:
* [XY RANGE | X-EYE ELEMENTS](https://nexmosphere.com/product-category/xy-range/)
* [XR-RANGE | WIRELESS PICK-UP DETECTION](https://nexmosphere.com/product-category/xr-range/)
* [XW RANGE | X-WAVE LED CONTROL](https://nexmosphere.com/product-category/xw-range/)
* [XT RANGE | TOUCH BUTTONS](https://nexmosphere.com/product-category/xt-range/)
* [XD RANGE | X-DOT ELEMENTS](https://nexmosphere.com/product-category/xd-range/)
* [XS-RANGE | X-SNAPPER ELEMENTS](https://nexmosphere.com/product-category/xs-range/)
* ...

It comes with a default Design Accelerator that will enable you to easily test your Nexmosphere device connection within Intuiface Composer.

![Nexmosphere Default Design Accelerator](Screenshots/Nexmosphere-DA.jpg "Nexmosphere Design Accelerator" "width:350px")

For more information about Nexmosphere and this interface asset, the online article about [using Nexmosphere within Intuiface](https://support.intuiface.com/hc/en-us/articles/360009681439) within the Intuiface Help Center.

# How to use the Nexmosphere Interface Asset?

To add the Nexmosphere Interface Asset into an Intuiface experience, follow these steps: 
* Close all running instances of **Intuiface Composer**. 
* Download the [latest released package here](https://github.com/intuiface/NexmosphereIA/releases).
* Extract the archive, open the resulting OutputInterfaceAsset folder, and copy the **Nexmosphere** folder to the path "[Drive]:\Users\\[UserName]\Documents\Intuiface\Interface Assets".
* Launch **Intuiface Composer** and open your project.
* Open the Interface Asset panel and select the **Add an Interface Asset** option when you enter "Nexmosphere" in the search bar, you will see the **Nexmosphere** Interface Asset.

# How to build this project?

**PREREQUISITES**: You must have Visual Studio and .NET installed.

The Nexmosphere Interface Asset is coded in C#.

To build this project, follow these steps:
* Open **Nexmosphere.sln** in Visual Studio 2013 or above,
* Build the solution in **Release** mode,
* Navigate to the root of the project and look for, a folder named **OutputInterfaceAsset** which contains the Nexmosphere Interface Asset.

If you want to make your own Interface Asset enhancements, review the article [Create a .NET Interface Asset](https://support.intuiface.com/hc/en-us/articles/360007179792-Create-a-NET-Interface-Asset) in the Intuiface Help Center.

-----

Copyright &copy; 2019 IntuiLab.

Released under the **MIT License**.

