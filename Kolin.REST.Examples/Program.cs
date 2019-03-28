using MFaaP.MFWSClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kolin.REST.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //ExamplesWithWrapperLibrary();

            //ExamplesWithoutWrapperLibrary();

        }

        public static void ExamplesWithoutWrapperLibrary()
        {
            var client = new RestSharp.RestClient(Constants.RestURL);

            //Authenticate
            var authRequest = new RestSharp.RestRequest("/REST/server/authenticationtokens", RestSharp.Method.POST);

            authRequest.AddJsonBody(new
            {
                Username = Constants.UserName,
                Password = Constants.Password,
                VaultGuid = Constants.VaultGUID
            });

            var tokenResponse = client.Execute<PrimitiveType<string>>(authRequest);

            var token = tokenResponse.Data.Value;

            //Get all employees
            var employeeRequest = new RestSharp.RestRequest($"/REST/Objects/{Constants.EmployeeObjectId}", RestSharp.Method.GET);

            employeeRequest.AddHeader("X-Authentication", token);

            var employeeResponse = client.Execute<Results<ObjectVersion>>(employeeRequest);

            var allEmployees = employeeResponse.Data.Items;

            var extSystemEmployeeId = Guid.NewGuid().ToString("N");

            //Create new employee using mandoroty fields
            var createEmployeeRequest = new RestSharp.RestRequest($"/REST/Objects/{Constants.EmployeeObjectId}", RestSharp.Method.POST);

            createEmployeeRequest.AddHeader("X-Authentication", token);
            createEmployeeRequest.AddJsonBody(new ObjectCreationInfo
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
            });

            var createdObjectResponse = client.Execute<ObjectVersion>(createEmployeeRequest);

            var createdObject = createdObjectResponse.Data;



            //Search with externalId
            var searchRequest = new RestSharp.RestRequest($"/REST/objects", RestSharp.Method.GET);

            //Construct URL Resource with objectTypeId and ExternalId property
            searchRequest.Resource += $"?o={Constants.EmployeeObjectId}&p{Constants.ExternalIdPropId}={extSystemEmployeeId}";

            searchRequest.AddHeader("X-Authentication", token);

            var searchResponse = client.Execute<Results<ObjectVersion>>(searchRequest);

            if (searchResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exceptionInfo = JsonConvert.DeserializeObject<WebServiceError>(searchResponse.Content);

                throw (null == exceptionInfo)
                            ? new Exception(searchResponse.Content)
                            : (Exception)exceptionInfo;
            }

            var foundEmployees = searchResponse.Data.Items;

            if (foundEmployees.Count == 0)
            {
                throw new Exception("No employees found");
            }

            var foundEmployee = foundEmployees[0];

            var checkedOutStatusRequest = new RestSharp.RestRequest(
                $"/REST/objects/{Constants.EmployeeObjectId}/{foundEmployee.ObjVer.ID}/latest/checkedout",
                RestSharp.Method.GET);
            checkedOutStatusRequest.AddHeader("X-Authentication", token);


            var checkedOutStatusResponse = client.Execute<MFCheckOutStatus>(checkedOutStatusRequest);

            if (checkedOutStatusResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exceptionInfo = JsonConvert.DeserializeObject<WebServiceError>(checkedOutStatusResponse.Content);

                throw (null == exceptionInfo)
                            ? new Exception(checkedOutStatusResponse.Content)
                            : (Exception)exceptionInfo;
            }

            var checkedOutStatus = checkedOutStatusResponse.Data;

            if (checkedOutStatus != MFCheckOutStatus.CheckedOutToMe && checkedOutStatus != MFCheckOutStatus.CheckedIn)
            {
                throw new Exception("Object is checked-out.");
            }

            ObjectVersion CheckedOutObject;

            if (checkedOutStatus != MFCheckOutStatus.CheckedOutToMe)
            {
                var checkOutRequest = new RestSharp.RestRequest(
               $"/REST/objects/{Constants.EmployeeObjectId}/{foundEmployee.ObjVer.ID}/latest/checkedout.aspx?_method=PUT",
               RestSharp.Method.POST);
                checkOutRequest.AddHeader("X-Authentication", token);

                var checkOutType = new PrimitiveType<MFCheckOutStatus>();
                checkOutType.Value = MFCheckOutStatus.CheckedOut;

                checkOutRequest.AddJsonBody(checkOutType);

                var checkOutResponse = client.Execute<ObjectVersion>(checkOutRequest);

                if (checkOutResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var exceptionInfo = JsonConvert.DeserializeObject<WebServiceError>(checkOutResponse.Content);

                    throw (null == exceptionInfo)
                                ? new Exception(checkOutResponse.Content)
                                : (Exception)exceptionInfo;
                }

                CheckedOutObject = checkOutResponse.Data;
            }
            else
            {
                CheckedOutObject = foundEmployee;
            }

            //Add a new field to employee object
            var tcknProp = new PropertyValue
            {
                PropertyDef = Constants.TCKNPropId,
                TypedValue = new TypedValue { DataType = MFDataType.Text, Value = "32472832270" }
            };

            //Construct the request
            var setNewValueRequest = new RestSharp.RestRequest(
                $"/REST/objects/{Constants.EmployeeObjectId}/{CheckedOutObject.ObjVer.ID}/latest/properties",
                RestSharp.Method.POST);

            //Add Header
            setNewValueRequest.AddHeader("X-Authentication", token);
            //Add body
            setNewValueRequest.AddJsonBody(new PropertyValue[] { tcknProp });

            //Post
            var setNewValueResponse = client.Execute<ExtendedObjectVersion>(setNewValueRequest);

            if (setNewValueResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exceptionInfo = JsonConvert.DeserializeObject<WebServiceError>(setNewValueResponse.Content);

                throw (null == exceptionInfo)
                            ? new Exception(setNewValueResponse.Content)
                            : (Exception)exceptionInfo;
            }

            var editedObjectVersion = setNewValueResponse.Data;

            var updateExistingFieldRequest = new RestSharp.RestRequest(
                $"/REST/objects/{Constants.EmployeeObjectId}/{CheckedOutObject.ObjVer.ID}/latest/properties",
                RestSharp.Method.POST);

            var nameProp = new PropertyValue
            {
                PropertyDef = Constants.NameSurnamePropId,
                TypedValue = new TypedValue { DataType = MFDataType.Text, Value = "Rıza Gökay Kıvırcıoğlu" }
            };

            //Add Header
            updateExistingFieldRequest.AddHeader("X-Authentication", token);
            //Add body
            updateExistingFieldRequest.AddJsonBody(new PropertyValue[] { nameProp });

            var updateExistingFieldResponse = client.Execute<ExtendedObjectVersion>(updateExistingFieldRequest);

            if (updateExistingFieldResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exceptionInfo = JsonConvert.DeserializeObject<WebServiceError>(updateExistingFieldResponse.Content);

                throw (null == exceptionInfo)
                            ? new Exception(updateExistingFieldResponse.Content)
                            : (Exception)exceptionInfo;
            }

            editedObjectVersion = updateExistingFieldResponse.Data;

            //Check-In the Object
            var checkInRequest = new RestSharp.RestRequest(
             $"/REST/objects/{Constants.EmployeeObjectId}/{foundEmployee.ObjVer.ID}/latest/checkedout.aspx?_method=PUT",
             RestSharp.Method.POST);

            checkInRequest.AddHeader("X-Authentication", token);

            checkInRequest.AddJsonBody(new PrimitiveType<MFCheckOutStatus>() { Value = MFCheckOutStatus.CheckedIn });

            var checkInResponse = client.Execute<ObjectVersion>(checkInRequest);

            if (checkInResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exceptionInfo = JsonConvert.DeserializeObject<WebServiceError>(checkInResponse.Content);

                throw (null == exceptionInfo)
                            ? new Exception(checkInResponse.Content)
                            : (Exception)exceptionInfo;
            }

            //Delete an object

            var deleteRequest = new RestSharp.RestRequest($"/objects/{Constants.EmployeeObjectId}/{foundEmployee.ObjVer.ID}/deleted.aspx?_method=PUT");

            deleteRequest.AddHeader("X-Authentication", token);
            deleteRequest.AddJsonBody(new PrimitiveType<bool>() { Value = true });

            var deleteResponse = client.Execute<ObjectVersion>(deleteRequest);

            if (deleteResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exceptionInfo = JsonConvert.DeserializeObject<WebServiceError>(deleteResponse.Content);

                throw (null == exceptionInfo)
                            ? new Exception(deleteResponse.Content)
                            : (Exception)exceptionInfo;
            }


            //Destroy an object
            var destroyRequest = new RestSharp.RestRequest($"/objects/{Constants.EmployeeObjectId}/{foundEmployee.ObjVer.ID}/lastest.aspx?_method=DELETE&allVersions=true", RestSharp.Method.POST);
           destroyRequest.AddHeader("X-Authentication", token);

            var destroyResponse = client.Execute<ObjectVersion>(destroyRequest);

            if (destroyResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var exceptionInfo = JsonConvert.DeserializeObject<WebServiceError>(destroyResponse.Content);

                throw (null == exceptionInfo)
                            ? new Exception(destroyResponse.Content)
                            : (Exception)exceptionInfo;
            }
        }

        public static void ExamplesWithWrapperLibrary()
        {

            //Initiate Client
            var client = new MFWSClient(Constants.RestURL);

            var guid = Guid.Parse(Constants.VaultGUID);

            //Authenticate
            client.AuthenticateUsingCredentials(guid, username: Constants.UserName, password: Constants.Password);

            //Search & Get All Employees
            var employees = client.ObjectSearchOperations.SearchForObjectsByConditions(
                new ObjectTypeSearchCondition(Constants.EmployeeObjectId));


            var extSystemEmployeeId = Guid.NewGuid().ToString("N");

            //Create new employee only with mandotory fields
            var createdEmployee = client.ObjectOperations.CreateNewObject(Constants.EmployeeObjectId, new ObjectCreationInfo
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
            }
            });

            //find an employee with external id
            var foundEmployees = client.ObjectSearchOperations.SearchForObjectsByConditions(
                new TextPropertyValueSearchCondition(Constants.ExternalIdPropId, extSystemEmployeeId));

            if (foundEmployees.Length == 0)
            {
                throw new Exception("No employees found");
            }

            var foundEmployee = foundEmployees[0];

            //Updating a field
            //Check if the object is checkedout or not?
            var checkOutStatus = client.ObjectOperations.GetCheckoutStatus(foundEmployee.ObjVer);


            if (checkOutStatus.HasValue)
            {
                if (checkOutStatus.Value != MFCheckOutStatus.CheckedOutToMe && checkOutStatus.Value != MFCheckOutStatus.CheckedIn)
                {
                    throw new Exception("Object is checked-out.");
                }
            }

            ObjectVersion CheckedOutObject;

            if (checkOutStatus.Value != MFCheckOutStatus.CheckedOutToMe)
            {
                //Check out if it is not checkedout by our app's user
                CheckedOutObject = client.ObjectOperations.CheckOut(foundEmployee.ObjVer);
            }
            else
            {
                CheckedOutObject = foundEmployee;
            }

            //Add a new field to employee object
            var editedObjectVersion = client.ObjectPropertyOperations.SetProperty(
                 CheckedOutObject.ObjVer,
                 new PropertyValue
                 {
                     PropertyDef = Constants.TCKNPropId,
                     TypedValue = new TypedValue { DataType = MFDataType.Text, Value = "32472832270" }
                 });

            //Update an existing field
            editedObjectVersion = client.ObjectPropertyOperations.SetProperty(
                editedObjectVersion.ObjVer,
                new PropertyValue
                {
                    PropertyDef = Constants.NameSurnamePropId,
                    TypedValue = new TypedValue { DataType = MFDataType.Text, Value = "Rıza Gökay Kıvırcıoğlu" }
                });

            //Check-In the object
            var editedEmployeeRecord = client.ObjectOperations.CheckIn(editedObjectVersion.ObjVer);

            //Deleting an employee
            var deletedRecord = client.ObjectOperations.DeleteObject(Constants.EmployeeObjectId, editedEmployeeRecord.ObjVer.ID);

            //Destroying a record
            client.ObjectOperations.DestroyObject(Constants.EmployeeObjectId, editedEmployeeRecord.ObjVer.ID, true, -1);
        }
    }
}
