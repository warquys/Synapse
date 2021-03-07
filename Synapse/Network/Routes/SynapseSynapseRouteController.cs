﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Swan.Formatters;
using Synapse.Network.Models;

namespace Synapse.Network.Routes
{
    public class SynapseSynapseRouteController : WebApiController
    {
        [Route(HttpVerbs.Get, "/ping")]
        public StatusedResponse Ping()
        {
            var clientData = this.GetClientData();
            if (clientData != null)
                SynapseNetworkServer.Instance.SyncedClientList[clientData.ClientUid] = DateTimeOffset.Now;

            return new PingResponse
            {
                Authenticated = clientData != null,
                Messages = clientData == null
                    ? new List<InstanceMessage>()
                    : SynapseNetworkServer.Instance.TakeAllMessages(clientData)
            };
        }

        [Route(HttpVerbs.Get, "/clients")]
        public StatusedResponse Clients()
        {
            var clientData = this.GetClientData();
            if (clientData == null) return StatusedResponse.Unauthorized;
            return new StatusListWrapper<string>(SynapseNetworkServer.Instance.SyncedClientList.Keys);
        }


        [Route(HttpVerbs.Post, "/post")]
        public async Task<StatusedResponse> Post()
        {
            var clientData = this.GetClientData();
            if (clientData == null) return StatusedResponse.Unauthorized;
            var msg = await HttpContext.GetRequestDataAsync<InstanceMessage>();
            if (msg.Receiver == "@")
            {
                var recv = new List<string>();
                foreach (var target in SynapseNetworkServer.Instance.TokenClientIDMap.Values.Where(x =>
                    x != clientData.ClientUid)
                )
                {
                    recv.Add(target);
                    SynapseNetworkServer.Instance.AddMessage(target, msg);
                }

                return new InstanceMessageTransmission
                {
                    Receivers = recv
                };
            }
            else
            {
                var results = SynapseNetworkServer.Instance.TokenClientIDMap.Values.Where(x => x == msg.Receiver);
                if (!results.Any())
                    return new InstanceMessageTransmission
                    {
                        Receivers = new List<string>()
                    };
                var recv = results.First();
                SynapseNetworkServer.Instance.AddMessage(recv, msg);
                return new InstanceMessageTransmission
                {
                    Receivers = new[] {recv}.ToList()
                };
            }

            return StatusedResponse.Success;
        }


        [Route(HttpVerbs.Post, "/handshake")]
        public async Task<StatusedResponse> Handshake()
        {
            var networkSyn = await HttpContext.GetRequestDataAsync<NetworkAuthSyn>();
            Server.Get.Logger.Info(
                $"Synapse-Network Handshake-Request from {networkSyn.ClientName}@{HttpContext.RemoteEndPoint}'");
            var data = new ClientData
            {
                Endpoint = HttpContext.RemoteEndPoint.Address.ToString(),
                PublicKey = RSA.Create(),
                ClientName = networkSyn.ClientName,
                ClientUid = Guid.NewGuid().ToString(),
                SessionToken = TokenFactory.Instance.GenerateShortToken(),
                CipherKey = TokenFactory.Instance.GenerateShortToken(),
                Valid = false
            };
            data.PublicKey.FromXmlString(networkSyn.PublicKey);
            SynapseNetworkServer.Instance.AddClient(data);
            Server.Get.Logger.Info("Synapse-Network Client to Cache'");
            return new NetworkAuthAck
            {
                ClientIdentifier = data.ClientUid,
                PublicKey = SynapseNetworkServer.Instance.PublicKey,
                MigrationPriority = 1
            };
        }

        [Route(HttpVerbs.Post, "/client/{id}/key")]
        public async Task<StatusedResponse> ExchangeKeys(string id)
        {
            var data = SynapseNetworkServer.Instance.DataById(id);
            data.ValidateEndpoint(this);
            Server.Get.Logger.Info(
                $"Synapse-Network KeyExchange-Request from {data.ClientName}:{data.ClientUid}@{HttpContext.RemoteEndPoint}");
            var keyExchange = await HttpContext.GetRequestDataAsync<NetworkAuthKeyExchange>();
            keyExchange.DecodeWithPrivate(SynapseNetworkServer.Instance.PrivateKey);
            data.ClientCipherKey = keyExchange.Key;

            var ownKeyExchange = new NetworkAuthKeyExchange
            {
                Key = data.CipherKey
            };
            ownKeyExchange.EncodeWithPublic(data.PublicKey);
            return ownKeyExchange;
        }

        [Route(HttpVerbs.Post, "/client/{id}/auth")]
        public async Task<string> Authenticate(string id)
        {
            var clientData = SynapseNetworkServer.Instance.DataById(id);
            clientData.ValidateEndpoint(this);
            Server.Get.Logger.Info(
                $"Auth-Request from {clientData.ClientName}:{clientData.ClientUid}@{HttpContext.RemoteEndPoint}");
            var raw = await HttpContext.GetRequestBodyAsStringAsync();
            var content = AESUtils.Decrypt(raw, clientData.CipherKey);
            var authAuthReq = Json.Deserialize<NetworkAuthReqAuth>(content);
            if (authAuthReq.ClientIdentifier != clientData.ClientUid)
            {
                Server.Get.Logger.Error($"Auth-Request from {HttpContext.RemoteEndPoint} has invalid ClientId");
                throw new HttpException(HttpStatusCode.Unauthorized);
            }

            if (SynapseNetworkServer.Instance.Secret == authAuthReq.Secret)
            {
                clientData.SessionToken = TokenFactory.Instance.GenerateShortToken();
                Server.Get.NetworkManager.Server.TokenClientIDMap[clientData.SessionToken] = clientData.ClientUid;
                clientData.Valid = true;
                Server.Get.Logger.Info($"Synapse-Network Auth-Request from {authAuthReq.ClientIdentifier} successful");
                var responseContent = Json.Serialize(new NetworkAuthResAuth
                {
                    SessionToken = clientData.SessionToken
                });
                responseContent = AESUtils.Encrypt(responseContent, clientData.ClientCipherKey);
                SynapseNetworkServer.Instance.SyncedClientList[clientData.ClientUid] = DateTimeOffset.Now;
                return responseContent;
            }

            throw new HttpException(HttpStatusCode.Unauthorized);
        }
    }
}