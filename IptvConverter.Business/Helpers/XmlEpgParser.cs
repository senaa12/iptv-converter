using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using IptvConverter.Business.Models;
using IptvConverter.Business.Utils;

namespace IptvConverter.Business.Helpers
{
    public class XmlEpgParser
    {
        public XmlEpgParser(Stream stream)
        {
            initializeFromStream(stream);
        }

        public string ErrorWhileReading { get; private set; }

        public List<EpgProgramme> Programe { get; private set; }

        public List<EpgChannel> Channels { get; private set; }

        public XmlEpg GetXmlEpg()
        {
            return XmlEpg.Create((Programe, Channels));
        }

        private void initializeFromStream(Stream stream)
        {
            Programe = new List<EpgProgramme>();
            Channels = new List<EpgChannel>();

            try
            {
                readStream(stream);
            }
            catch (Exception ex)
            {
                ErrorWhileReading = ex.Message;
            }
        }

        /// <summary>
        /// return whole program from file, only for yesterday, today and tommorow
        /// </summary>
        /// <returns></returns>
        private void readStream(Stream stream)
        {
            var currentTime = DateTimeUtils.GetZagrebCurrentDateTime();

            EpgProgramme _currentItem = null;
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
                                    if (reader.Name.Equals("start"))
                                    {
                                        _currentItem.Start = reader.Value;
                                    }
                                    else if (reader.Name.Equals("stop"))
                                    {
                                        _currentItem.End = reader.Value;
                                    }
                                    else if (reader.Name.Equals("channel"))
                                    {
                                        _currentItem.ChannelId = reader.Value;
                                    }
                                }

                                if (!(_currentItem.StartDate.Date > currentTime.AddDays(2).Date || _currentItem.StartDate < currentTime.AddDays(2).Date))
                                {
                                    _currentItem = null;
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
                                if (_currentItem.Directors == null)
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
                                    if (reader.Name.Equals("src"))
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
                                Programe.Add(_currentItem);
                                _currentItem = null;
                            }
                            else if (reader.Name.Equals("channel") && _currentChannel != null)
                            {
                                Channels.Add(_currentChannel);
                                _currentChannel = null;
                            }

                            break;
                    }
                }
            }

        }
    }
}
