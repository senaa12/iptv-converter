using IptvConverter.Business.Models;
using IptvConverter.Business.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace IptvConverter.Business.Helpers
{
    public class XmlEpgParser
    {
        public XmlEpgParser(Stream stream)
        {
            var result = getProgrammeAndChannels(stream);
            Programe = result.programme;
            Channels = result.channels;
        }

        public List<EpgProgramme> Programe { get; private set; }
       
        public List<EpgChannel> Channels { get; private set; }

        public List<EpgProgramme> GetProgrammeForChannel(string channelId)
        {
            return Programe.Where(x => x.ChannelId.Equals(channelId)).ToList();
        }

        public bool ChannelExists(string channelId)
        {
            return Channels.Any(x =>
                (string.IsNullOrEmpty(x.ChannelEpgId) && channelId.Equals(x.ChannelEpgId, System.StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrEmpty(x.Name) && channelId.Equals(x.Name, System.StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// return whole program from file, only for yesterday, today and tommorow
        /// </summary>
        /// <returns></returns>
        private (List<EpgProgramme> programme, List<EpgChannel> channels) getProgrammeAndChannels(Stream stream)
        {
            var currentTime = DateTimeUtils.GetZagrebCurrentDateTime();

            var result = new List<EpgProgramme>();
            EpgProgramme _currentItem = null;

            var channelsResult = new List<EpgChannel>();
            EpgChannel _currentChannel = null;

            using (var reader = new XmlTextReader(stream))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            if (reader.Name.Equals("programme"))
                            {
                                _currentItem = new EpgProgramme();
                                while (reader.MoveToNextAttribute())
                                {
                                    if(reader.Name.Equals("start"))
                                    {
                                        _currentItem.Start = reader.Value;
                                    }
                                    else if(reader.Name.Equals("stop"))
                                    {
                                        _currentItem.End = reader.Value;
                                    }
                                    else if(reader.Name.Equals("channel"))
                                    {
                                        if(!(_currentItem.StartDate.Date > currentTime.AddDays(2).Date || _currentItem.StartDate < currentTime.AddDays(2).Date))
                                        {
                                            _currentItem = null;
                                        }
                                        else
                                        {
                                            _currentItem.ChannelId = reader.Value;
                                        }
                                    }
                                }
                                break;
                            }
                            else if (_currentItem != null && reader.Name.Equals("title"))
                            {
                                reader.Read();
                                _currentItem.Title = reader.Value;
                            }
                            else if (_currentItem != null && reader.Name.Equals("desc"))
                            {
                                reader.Read();
                                _currentItem.Description = reader.Value;
                            }
                            else if (_currentItem != null && reader.Name.Equals("director"))
                            {
                                reader.Read();
                                if(_currentItem.Directors == null)
                                {
                                    _currentItem.Directors = new List<string>();
                                }
                                _currentItem.Directors.Add(reader.Value);
                            }
                            else if (_currentItem != null && reader.Name.Equals("actor"))
                            {
                                reader.Read();
                                if (_currentItem.Actors == null)
                                {
                                    _currentItem.Actors = new List<string>();
                                }
                                _currentItem.Actors.Add(reader.Value);
                            }
                            else if (_currentItem != null && reader.Name.Equals("category"))
                            {
                                reader.Read();
                                if (_currentItem.Category == null)
                                {
                                    _currentItem.Category = new List<string>();
                                }
                                _currentItem.Category.Add(reader.Value);
                            }
                            else if (_currentItem != null && reader.Name.Equals("icon"))
                            {
                                reader.Read();
                                while (reader.MoveToNextAttribute())
                                {
                                    if(reader.Name.Equals("src"))
                                    {
                                        _currentItem.Icon = reader.Value;
                                    }
                                }
                            }
                            else if (reader.Name.Equals("channel"))
                            {
                                _currentChannel = new EpgChannel();
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name.Equals("id"))
                                    {
                                        _currentChannel.ChannelEpgId = reader.Value;
                                    }
                                }
                            }
                            else if (reader.Name.Equals("icon") && _currentChannel != null)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (reader.Name.Equals("src"))
                                    {
                                        _currentChannel.Logo = reader.Value;
                                    }
                                }
                            }
                            else if (reader.Name.Equals("display-name") && _currentChannel != null)
                            {
                                reader.Read();
                                _currentChannel.Name = reader.Value;
                            }
                            else if (reader.Name.Equals("url") && _currentChannel != null)
                            {
                                reader.Read();
                                _currentChannel.Url = reader.Value;
                            }

                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name.Equals("programme") && _currentItem != null)
                            {
                                result.Add(_currentItem);
                                _currentItem = null;
                            }
                            else if (reader.Name.Equals("channel") && _currentChannel != null)
                                {
                                    channelsResult.Add(_currentChannel);
                                    _currentChannel = null;
                                }

                            break;
                    }
                }
            }

            return (result, channelsResult);
        }
    }
}
