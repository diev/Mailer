// Copyright (c) 2017-2021 Dmitrii Evdokimov. All rights reserved.
// Licensed under the Apache License, Version 2.0.
// Source https://github.com/diev/

using System.Net.NetworkInformation;

namespace Lib
{
    public class Gateway
    {
        public static string DefaultGateway()
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                if (adapter.OperationalStatus != OperationalStatus.Up)
                    continue;
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                if (adapterProperties == null)
                    continue;

                GatewayIPAddressInformationCollection gateways = adapterProperties.GatewayAddresses;
                if (gateways.Count > 0)
                {
                    foreach (GatewayIPAddressInformation address in gateways)
                    {
                        string gw = address.Address.ToString();
                        if (gw.Equals("0.0.0.0"))
                            continue;

                        return gw;
                    }
                }
            }
            return null;
        }
    }
}
