using System;
using System.Diagnostics;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFilesAPI;
using Newtonsoft.Json;

namespace Kolin.ExtensionOperations.VAF
{
    /// <summary>
    /// The entry point for this Vault Application Framework application.
    /// </summary>
    /// <remarks>Examples and further information available on the developer portal: http://developer.m-files.com/. </remarks>
    public class VaultApplication
        : VaultApplicationBase
    {

        [VaultExtensionMethod("SetExternalId", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        public string SetExternalID(EventHandlerEnvironment env)
        {
            string response;
            ResponseMessage message = new ResponseMessage();

            try
            {

                SetExternalIdRequest requestData;

                try
                {
                    requestData = JsonConvert.DeserializeObject<SetExternalIdRequest>(env.Input);
                }
                catch (Exception Ex)
                {
                    SysUtils.ReportErrorToEventLog(Ex);
                    message.Code = 500;
                    message.Description = "Input was not in expected format.";

                    response = JsonConvert.SerializeObject(message);
                    return response;
                }

                var objID = new ObjID();
                objID.Type = Convert.ToInt32(requestData.ObjectTypeId);
                objID.ID = Convert.ToInt32(requestData.InternalId);


                var vault = env.Vault;

                vault.ObjectOperations.SetExternalID(objID, requestData.ExternalId);


                message.Code = 200;
                message.Description = "Successfully set external id.";
                response = JsonConvert.SerializeObject(message);

            }
            catch (Exception Ex)
            {

                message.Code = 500;
                message.Description = Ex.Message;
                message.ResponseObject = null;

                response = JsonConvert.SerializeObject(message);
            }

            return response;

        }

        [VaultExtensionMethod("RenameValueListItem", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone)]
        public string RenameValueListItem(EventHandlerEnvironment env)
        {
            string response;
            ResponseMessage message = new ResponseMessage();

            try
            {

                RenameValueListItemRequest requestData;

                try
                {
                    requestData = JsonConvert.DeserializeObject<RenameValueListItemRequest>(env.Input);
                }
                catch (Exception Ex)
                {
                    SysUtils.ReportErrorToEventLog(Ex);
                    message.Code = 500;
                    message.Description = "Input was not in expected format.";

                    response = JsonConvert.SerializeObject(message);
                    return response;
                }

                ValueListItem valueListItem;

                if (requestData.IsDisplayID)
                {
                    valueListItem = PermanentVault.ValueListItemOperations.GetValueListItemByDisplayID(requestData.ValueListId, requestData.ItemId);
                }
                else
                {
                    valueListItem = PermanentVault.ValueListItemOperations.GetValueListItemByID(requestData.ValueListId, Convert.ToInt32(requestData.ItemId));
                }

                valueListItem.Name = requestData.Name;

                PermanentVault.ValueListItemOperations.UpdateValueListItem(valueListItem);

                message.ResponseObject = valueListItem;
                message.Code = 200;
                message.Description = "Successfully renamed item.";
                response = JsonConvert.SerializeObject(message);

            }
            catch (Exception Ex)
            {

                message.Code = 500;
                message.Description = Ex.Message;
                message.ResponseObject = null;

                response = JsonConvert.SerializeObject(message);
            }


            return response;
        }


    }
}