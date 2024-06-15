using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Security.Cryptography.Xml;
using ClockTransactionsTransmiter.ViewModels;

namespace ClockTransactionsTransmiter.Helper
{
    public static class UdpSearchDataHelper
    {
        public static List<DeviceViewModel> ToDevices(IntPtr pBuff)
        {
            List<DeviceViewModel> devices = new List<DeviceViewModel>();

            AnvizNew.CCHEX_UDP_SEARCH_ALL_STRU_EXT_INF result;
            result = (AnvizNew.CCHEX_UDP_SEARCH_ALL_STRU_EXT_INF)Marshal.PtrToStructure(pBuff, typeof(AnvizNew.CCHEX_UDP_SEARCH_ALL_STRU_EXT_INF));
            
            if (result.DevNum <= 0)
            {
                return devices;
            }

            IntPtr pTmp;
            pTmp = pBuff + 4;
            DeviceViewModel device = null;
            for (int i = 0; i < result.DevNum; i++)
            {
                device = new DeviceViewModel();
                AnvizNew.CCHEX_UDP_SEARCH_STRU_EXT_INF one_dev_info = result.dev_net_info[i];
                device.Conencted = one_dev_info.Result == 0;
                device.Index = i + 1;
                device.MachineId = one_dev_info.MachineId.ToString(); 
                device.DevSerialNum = "";
                device.IpAddr = "";
                device.MacAddr = "";

                switch (one_dev_info.DevHardwareType)
                {
                    case (int)AnvizNew.NetCardType.NETCARD_WITHOUT_DNS:
                        AnvizNew.CCHEX_UDP_SEARCH_STRU withoutDns;
                        withoutDns = (AnvizNew.CCHEX_UDP_SEARCH_STRU)Marshal.PtrToStructure(pTmp, typeof(AnvizNew.CCHEX_UDP_SEARCH_STRU));
                        device.DevSerialNum = BytesStringHelper.Encoding.GetString(withoutDns.DevSerialNum);
                        device.IpAddr = BytesStringHelper.BytesToIpAddr(withoutDns.DevNetInfo.IpAddr);
                        device.MacAddr = BytesStringHelper.BytesToMacAddr(withoutDns.DevNetInfo.MacAddr);
                        break;
                    case (int)AnvizNew.NetCardType.NETCARD_WITH_DNS:
                        AnvizNew.CCHEX_UDP_SEARCH_WITH_DNS_STRU withDns;
                        withDns = (AnvizNew.CCHEX_UDP_SEARCH_WITH_DNS_STRU)Marshal.PtrToStructure(pTmp, typeof(AnvizNew.CCHEX_UDP_SEARCH_WITH_DNS_STRU));
                        device.DevSerialNum = BytesStringHelper.Encoding.GetString(withDns.BasicSearch.DevSerialNum);
                        device.IpAddr = BytesStringHelper.BytesToIpAddr(withDns.BasicSearch.DevNetInfo.IpAddr);
                        device.MacAddr = BytesStringHelper.BytesToMacAddr(withDns.BasicSearch.DevNetInfo.MacAddr);
                        break;
                    case (int)AnvizNew.NetCardType.NETCARD_TWO_CARD:
                        AnvizNew.CCHEX_UDP_SEARCH_TWO_CARD_STRU withTwoCard;
                        withTwoCard = (AnvizNew.CCHEX_UDP_SEARCH_TWO_CARD_STRU)Marshal.PtrToStructure(pTmp, typeof(AnvizNew.CCHEX_UDP_SEARCH_TWO_CARD_STRU));
                        device.DevSerialNum = BytesStringHelper.Encoding.GetString(withTwoCard.DevSerialNum);
                        device.IpAddr = BytesStringHelper.BytesToIpAddr(withTwoCard.CardInfo[0].IpAddr);
                        device.MacAddr = BytesStringHelper.BytesToMacAddr(withTwoCard.CardInfo[0].MacAddr);
                        break;
                    default:
                        break;
                }

                devices.Add(device);
                pTmp = pTmp + Marshal.SizeOf(one_dev_info);
            }

            return devices;
        }
    }
}