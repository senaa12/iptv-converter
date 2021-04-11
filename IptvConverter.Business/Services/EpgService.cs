using IptvConverter.Business.Models;
using IptvConverter.Business.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace IptvConverter.Business.Services
{
    public class EpgService : IEpgService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        public EpgService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<EpgChannelExtended>> GetEpgServiceChannels(string serviceUrl, bool fillCustomData = true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, serviceUrl);

            var client = _httpClientFactory.CreateClient();
            var response = await client.SendAsync(request);

            var list = parseStream(await response.Content.ReadAsStreamAsync());
            if(!fillCustomData)
            {
                return list.Select(x => new EpgChannelExtended(x, null)).ToList();
            }

            return list.Select(x =>
            {
                var match = Config.ChannelsConfig.Instance.MatchChannelByName(x.Name);
                return new EpgChannelExtended(x, match?.ID);
            }).OrderBy(x => x.ChannelId).ToList();
        }

        private List<EpgChannel> parseStream(Stream streamInput)
        {
            var result = new List<EpgChannel>();
            EpgChannel _currentItem = null;
            var displayNameTagOpen = false;
            var urlTagOpen = false;

            using (var reader = new XmlTextReader(streamInput))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if(reader.Name.Equals("channel"))
                            {
                                _currentItem = new EpgChannel();
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name.Equals("id"))
                                    {
                                        _currentItem.ChannelEpgId = reader.Value;
                                        break;
                                    }
                                }
                                break;
                            }

                            if(reader.Name.Equals("display-name") && _currentItem != null)
                            {
                                displayNameTagOpen = true;
                                break;
                            }

                            if (reader.Name.Equals("icon") && _currentItem != null)
                            {
                                while(reader.MoveToNextAttribute())
                                {
                                    if(reader.Name.Equals("src"))
                                    {
                                        _currentItem.Logo = reader.Value;
                                        break;
                                    }
                                }
                            }

                            if(reader.Name.Equals("url") && _currentItem != null)
                            {
                                urlTagOpen = true;
                                break;
                            }
                            break;
                        case XmlNodeType.Text:
                            if (displayNameTagOpen)
                            {
                                _currentItem.Name = reader.Value;
                                displayNameTagOpen = false;
                                break;
                            }

                            if (urlTagOpen)
                            {
                                _currentItem.Url = reader.Value;
                                urlTagOpen = false;
                                break;
                            }

                            break;    
                        case XmlNodeType.EndElement: 
                            if(reader.Name.Equals("channel") && _currentItem != null)
                            {
                                result.Add(_currentItem);
                                _currentItem = null;
                                break;
                            }

                            break;
                    }
                }
            }

            return result;
        }
    }
}
