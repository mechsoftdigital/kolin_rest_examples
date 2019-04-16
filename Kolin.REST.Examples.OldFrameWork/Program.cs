using MFiles.Mfws.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Kolin.REST.Examples.OldFrameWork
{
    class Program
    {
        static void Main(string[] args)
        {

            var auth = new Authentication
            {
                Username = Constants.UserName,
                Password = Constants.Password,
                WindowsUser = false,  // Change to 'true' if using Windows-credentials.
                VaultGuid = Constants.VaultGUID  // Use GUID format with {braces}.
            };

            // Create the web request.
            var request = CreateRequest($"{Constants.RestURL}/REST/server/authenticationtokens.aspx", "POST", auth);

            // Get the response.
            var response = (HttpWebResponse)request.GetResponse();

            // Deserialize the authentication token.
            var deserializer = new DataContractJsonSerializer(typeof(PrimitiveType<string>));
            var token = (PrimitiveType<string>)deserializer.ReadObject(response.GetResponseStream());


            //GET All Employees
            var getAllEmployeePath = $"{Constants.RestURL}/REST/Objects/{Constants.EmployeeObjectId}";

            request = CreateRequest(getAllEmployeePath, "GET");
            request.Headers["X-Authentication"] = token.Value;

            // Get the response.
            response = (HttpWebResponse)request.GetResponse();

            deserializer = new DataContractJsonSerializer(typeof(Results<ObjectVersion>));

            var result = (Results<ObjectVersion>)deserializer.ReadObject(response.GetResponseStream());

            var extSystemEmployeeId = Guid.NewGuid().ToString("N");

            //Create employee
            var creationInfo = new ObjectCreationInfo
            {
                PropertyValues = new PropertyValue[] {

                    //Employee class
                     new PropertyValue{
                        PropertyDef = 100, //Built-in class ID not parametrical
                        TypedValue = new TypedValue{DataType = MFDataType.Lookup, HasValue=true, Lookup = new Lookup{ Version = -1, Item = Constants.EmployeeClassId} } },

                    //Name 
                    new PropertyValue{
                        PropertyDef = Constants.NameSurnamePropId,
                        TypedValue = new TypedValue{DataType = MFDataType.Text, Value = "Gökay Kıvırcıoğlu" } },

                    //Working status
                    new PropertyValue {
                        PropertyDef = Constants.WorkingStatusPropId,
                        TypedValue = new TypedValue{DataType = MFDataType.Lookup, Lookup = new Lookup{Item = Constants.WorkingStatusActiveId, Version = -1 }  }
                    },

                    //Set External System' s ID
                    new PropertyValue{
                        PropertyDef = Constants.ExternalIdPropId,
                        TypedValue = new TypedValue{ DataType = MFDataType.Text, Value = extSystemEmployeeId }
                    }
            },

                Files = new UploadInfo[] { }
            };

            var createPath = $"{Constants.RestURL}/REST/Objects/{Constants.EmployeeObjectId}";

            //Create Employee Object
            request = CreateRequest(createPath, "POST", creationInfo);
            request.Headers["X-Authentication"] = token.Value;

            // Get the response.
            response = (HttpWebResponse)request.GetResponse();

            deserializer = new DataContractJsonSerializer(typeof(ObjectVersion));

            var employeeResult = (ObjectVersion)deserializer.ReadObject(response.GetResponseStream());

            //Crate a valuelist item
            var valueListCreatePath = $"{Constants.RestURL}/REST/valuelists/{Constants.JobsValueListId}/items";

            var vlItem = new ValueListItem { Name = "Test Yeni Değer" };

            request = CreateRequest(valueListCreatePath, "POST", vlItem);
            request.Headers["X-Authentication"] = token.Value;

            // Get the response.
            response = (HttpWebResponse)request.GetResponse();

            deserializer = new DataContractJsonSerializer(typeof(ValueListItem));

            var valueListResult = (ValueListItem)deserializer.ReadObject(response.GetResponseStream());

            var internalIdOfValueListItem = valueListResult.ID;


        }

        private static WebRequest CreateRequest(string path, string method, object data = null)
        {
            // Create the web request.
            var request = (HttpWebRequest)WebRequest.Create(path);
            request.Method = method;

            if (data != null)
            {
                // Serialize the authentication details into the request.
                var serializer = new DataContractJsonSerializer(data.GetType());

                // .NET 4.0 and above way of writing stuff to Request.!
                // serializer.WriteObject(request.GetRequestStream(), data);

                //  .NET 3.5 and below  way of Writing stuff to Request.!
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, data);
                    string json = Encoding.Default.GetString(ms.ToArray());
                    byte[] byteArray = Encoding.UTF8.GetBytes(json);

                    request.ContentType = "application/json";

                    using (var dataStream = request.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);

                    }

                }
            }

            return request;


        }


    }
}
