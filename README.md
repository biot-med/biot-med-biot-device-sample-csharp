# README #

BioT Device Sample CSharp is a sample code of how to comunicate with BioT in a C# code

### Preaparing Working Environment ###
* Make sure you have at least .NET 2.0 installed 
```dotnet --version```
* For debugging C# in VSCode do the following
In VSCode Select View-> Command Pallet and select ".NET Generate Assets for Built and Debug"
* For compiling the code do
'dotnet build'
* For publishing a release do
```dotnet publish --configuration Release```

### Downloading and Preparing Certificates ###
1. [Add device](https://docs.biot-med.com/docs/adding-a-new-device#step-2---add-device-page) in the BioT Manufacturer Portal 
2. [Generate certificate](https://docs.biot-med.com/docs/generate-and-download-a-permanent-device-security-certificate) from the BioT Manufacturer Portal
3. Change the following file extension:
     - …caCertificate.txt --> …caCertificate.pem
     - …certificate.txt --> …certificate.pem.crt
     - …privateKey.txt --> …privateKey.pem.key
4. In order to establish an MQTT connection with BioT, the root CA certificate, private key of the thing and the certificate of the thing are needed. The .NET Cryptographic APIs can understand root CA (.crt) and device private key (.key) out-of-the-box. It expects the device certificate to be in the .pfx format, not in the .pem format. Hence we need to convert the device certificate from .pem to .pfx:
```openssl pkcs12 -export -in ..certificate.crt -inkey ..privateKey.key -out ..certificate.cert.pfx -certfile ..caCertificate.cert```
5. Copy the certificate files to  ```{working directory}/BioT.DeviceSample/certs``` folder

### Configuring the Environment ###
1. Open and edit the ```{working directory}/BioT.DeviceSample/App.Config``` and update the the configuration as needed
2. Compile and run the project

### Examples in code ###
1. Getting Remote Configuration: https://docs.biot-med.com/docs/remote-configuration
     - Log into the Manufacturer portal and change edit the device configuration https://docs.biot-med.com/docs/viewing-and-editing-devices
     - View the logs catching the event
2. Getting a JWT Token
     - View the logs of how JWT is recieved via MQTT
3. Sending Boot Time
     - Log into the the Device and view how the BootTime propery is being updated
4. Uploading a file to the Device 
     - In BioT Console, Edit the device template and create a General File of a File Type
     - Name it Device Configuration General, Update the JSON Name to be device_configuration_general
     - There are 3 samples file located in /assets folder, select one and put it in the app.config as under "assets-sample-file"
     - Once the Application is running, it will upload and attach the file to the device