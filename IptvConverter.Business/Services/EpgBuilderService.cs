using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IptvConverter.Business.Helpers;
using IptvConverter.Business.Models;
using IptvConverter.Business.Utils;
using Microsoft.Extensions.Logging;

namespace IptvConverter.Business.Services
{
    public class EpgBuilderService
    {
        private readonly EpgService _epgService;
        private readonly string _uploadFolder;

        private EpgBuilderService(EpgService epgService, string uploadFolder)
        {
            _epgService = epgService;
            _uploadFolder = uploadFolder;
        }

        public static EpgBuilderService Configure(EpgService epgService, string uploadFolder)
        {
            return new EpgBuilderService(epgService, uploadFolder);
        }

        public async Task Build()
        {
            using (FileStream fileStream = File.Create(Path.Combine(_uploadFolder, "guide.xml")))
            {
                var epgXml = XmlEpg.Create();

                var baseEpg = await _epgService.FetchXmlEpg("http://cdn.iptvhr.net/tvdata/guide.xml");

                baseEpg = await fetchPhoenixRebborn(baseEpg);

                epgXml.AddChannels(baseEpg.Channels, baseEpg.Programme);

                epgXml = await fetchMojProfil(epgXml);

                epgXml = await fetchSerbianForumEpg(epgXml);

                epgXml = await fetchAllMojTvChannels(epgXml);

                epgXml = fixEpgIdToM3u4u(epgXml);

                await fileStream.WriteAsync(epgXml.GenerateEpgFile());
                await fileStream.FlushAsync();
            }
        }

        private async Task<XmlEpg> fetchMojProfil(XmlEpg baseEpg)
        {
            try
            {
                string channelId;

                channelId = "RTL Televizija";
                if (!baseEpg.ExistsProgrammeForChannel(channelId))
                {
                    var profil = await _epgService.FetchXmlEpg("https://tvprofil.net/xmltv/data/rtl.hr/weekly_rtl.hr_tvprofil.net.xml");
                    baseEpg.AddChannel(profil.Channels.First(), profil.GetProgrammeForChannel(profil.Channels.First().ChannelEpgId));
                }

                return baseEpg;
            }
            catch (Exception ex)
            {
                _epgService.Logger.LogInformation($"MOJ PROFIL FAIL: {ex.Message}");
                return baseEpg;
            }
        }

        private async Task<XmlEpg> fetchPhoenixRebborn(XmlEpg baseEpg)
        {
            var phoenixRebborn = await _epgService.FetchEpgGzip("https://epg.phoenixrebornbuild.com.hr/");
            if (baseEpg.Programme.Count < 16000)
            {
                return phoenixRebborn;
            }

            var channelsToMerge = new List<string>
            {
                "RTL Televizija",
                "Discovery Science",
                "HBO1",
                "HBO2",
                "HBO2",
                "Arena Sport 5",
                "Arena Sport 6",
                "Food Network",
                "24 Kitchen",
                "SportKlub HD",
                "SportKlub 1",
                "SportKlub 2",
                "SportKlub 3",
                "SportKlub 4",
                "SportKlub 5",
                "SportKlub 6",
                "Eurosport 1",
                "Eurosport 2",
                "HNTV"
            };

            foreach (var ch in channelsToMerge)
            {
                if (!baseEpg.ExistsProgrammeForChannel(ch)
                    && phoenixRebborn.ChannelExists(ch, out EpgChannel phoenixCh)
                    && phoenixRebborn.ExistsProgrammeForChannel(ch))
                {
                    baseEpg.AddChannel(phoenixCh, phoenixRebborn.GetProgrammeForChannel(phoenixCh.ChannelEpgId));
                }
            }


            return baseEpg;
        }

        private XmlEpg fixEpgIdToM3u4u(XmlEpg epgXml)
        {
            // HRVATSKI
            epgXml.ChangeEpgIdForChannel("HRT 1", "HTV1(src01).hr");
            epgXml.ChangeEpgIdForChannel("HRT 2", "HTV2(src01).hr");
            epgXml.ChangeEpgIdForChannel("HRT 3", "HTV3(src01).hr");
            epgXml.ChangeEpgIdForChannel("HRT 4", "HTV4(src01).hr");
            epgXml.ChangeEpgIdForChannel("RTL", "RTLTELEVIZIJA(src01).hr");
            epgXml.ChangeEpgIdForChannel("RTL2", "RTL2(src01).hr");
            epgXml.ChangeEpgIdForChannel("NOVA TV", "NOVATV(src01).hr");
            epgXml.ChangeEpgIdForChannel("DOMA TV", "DomaTV(src01).hr");

            // FILMOVI
            epgXml.ChangeEpgIdForChannel("HBO", "HBO(src01).hr");
            epgXml.ChangeEpgIdForChannel("HBO 2", "HBO2(src01).hr");
            epgXml.ChangeEpgIdForChannel("HBO 3", "HBO3(src01).hr");
            epgXml.ChangeEpgIdForChannel("Cinemax 1", "CMAX(src01).hr");
            epgXml.ChangeEpgIdForChannel("Cinemax 2", "CMAX2(src01).hr");
            epgXml.ChangeEpgIdForChannel("Cinestar Premiere 1", "CineStarTVPremiere1(src01).hr");
            epgXml.ChangeEpgIdForChannel("Cinestar Premiere 2", "CineStarTVPremiere2(src01).hr");
            epgXml.ChangeEpgIdForChannel("CineStar TV", "CinestarTV(src01).hr");
            epgXml.ChangeEpgIdForChannel("CineStar TV 2", "CinestarTV2(src01).hr");
            epgXml.ChangeEpgIdForChannel("CineStar Fantasy", "CinestarTVFantasy(src01).hr");
            epgXml.ChangeEpgIdForChannel("CineStar Action", "CinestarActionandThriller(src01).hr");
            epgXml.ChangeEpgIdForChannel("CineStar Comedy", "CineStarTVComedyFamily(src01).hr");
            epgXml.ChangeEpgIdForChannel("FOX TV", "Fox(src01).hr");
            epgXml.ChangeEpgIdForChannel("FOX Life", "FoxLife(src01).hr");
            epgXml.ChangeEpgIdForChannel("FOX Crime", "FoxCrime(src01).hr");
            epgXml.ChangeEpgIdForChannel("FOX Movies", "FoxMovies(src01).hr");

            // SPORT
            epgXml.ChangeEpgIdForChannel("Arena Sport 1", "ArenaSport1(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 2", "ArenaSport2(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 3", "ArenaSport3(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 4", "ArenaSport4(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 5", "ArenaSport5(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 6", "ArenaSport6(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 7", "ArenaSport7(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 8", "ArenaSport8(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 9", "ArenaSport9(src01).hr");
            epgXml.ChangeEpgIdForChannel("Arena Sport 10", "ArenaSport10(src01).hr");
            //epgXml.ChangeEpgIdForChannel("SportKlub HD", "SportKlub HD");
            epgXml.ChangeEpgIdForChannel("SportKlub 1", "SportKlub1(src01).hr");
            epgXml.ChangeEpgIdForChannel("SportKlub 2", "SportKlub2(src01).hr");
            epgXml.ChangeEpgIdForChannel("SportKlub 3", "SportKlub3(src01).hr");
            epgXml.ChangeEpgIdForChannel("SportKlub 4", "SportKlub4(src01).hr");
            epgXml.ChangeEpgIdForChannel("SportKlub 5", "SportKlub5(src01).hr");
            epgXml.ChangeEpgIdForChannel("SportKlub 6", "SportKlub6(src01).hr");
            epgXml.ChangeEpgIdForChannel("Eurosport 1", "Eurosport1(src01).hr");
            epgXml.ChangeEpgIdForChannel("Eurosport 2", "Eurosport2(src01).hr");
            epgXml.ChangeEpgIdForChannel("HNTV", "HNTV(src01).hr");

            // DOKUMENTARNI
            epgXml.ChangeEpgIdForChannel("National Geographic", "NationalGeographic(src01).hr");
            epgXml.ChangeEpgIdForChannel("Animal Planet", "AnimalPlanet(src01).hr");
            epgXml.ChangeEpgIdForChannel("National Geographic Wild", "NatGeoWild(src01).hr");
            epgXml.ChangeEpgIdForChannel("Discovery Channel", "DiscoveryChannel(src01).hr");
            epgXml.ChangeEpgIdForChannel("Discovery Science", "Discovery Science");
            epgXml.ChangeEpgIdForChannel("BBC Earth", "BBCEarth(src01).hr");
            epgXml.ChangeEpgIdForChannel("History Channel", "HistoryChannel(src01).hr");
            epgXml.ChangeEpgIdForChannel("History Channel 2", "HistoryChannel2(src01).hr");
            epgXml.ChangeEpgIdForChannel("Discovery Channel", "DiscoveryChannel(src01).hr");
            epgXml.ChangeEpgIdForChannel("Food Network", "FoodNetwork(src01).hr");
            epgXml.ChangeEpgIdForChannel("24 Kitchen", "24Kitchen(src01).hr");

            // MUZICKI
            epgXml.ChangeEpgIdForChannel("Balkanika TV", "BALKANIKAMUSICTV(src01).hr");
            epgXml.ChangeEpgIdForChannel("MTV MUSIC", "MTV(src01).hr");
            epgXml.ChangeEpgIdForChannel("CMC", "CMC(src01).hr");

            return epgXml;
        }

        private async Task<XmlEpg> fetchSerbianForumEpg(XmlEpg epgXml)
        {
            var serbianForum = await _epgService.FetchEpgGzip("http://epg.serbianforum.org/losmij/epg.xml.gz");
            var configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("RTS 1");
            var rts1 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
            if (!epgXml.ExistsProgrammeForChannel(configChannel.EpgId) && rts1 != null)
            {
                epgXml.AddChannel(rts1, serbianForum.GetProgrammeForChannel(rts1.ChannelEpgId));
            }

            configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("RTS 2");
            var rts2 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
            if (!epgXml.ExistsProgrammeForChannel(configChannel.EpgId) && rts2 != null)
            {
                epgXml.AddChannel(rts2, serbianForum.GetProgrammeForChannel(rts2.ChannelEpgId));
            }

            configChannel = Config.ChannelsConfig.Instance.MatchChannelByName("BHT 1");
            var bht1 = serbianForum.Channels.FirstOrDefault(x => string.Equals(x.ChannelEpgId, configChannel.EpgId, StringComparison.OrdinalIgnoreCase));
            if (!epgXml.ExistsProgrammeForChannel(configChannel.EpgId) && bht1 != null)
            {
                epgXml.AddChannel(bht1, serbianForum.GetProgrammeForChannel(bht1.ChannelEpgId));
            }

            return epgXml;
        }

        private async Task<XmlEpg> fetchAllMojTvChannels(XmlEpg epgXml)
        {
            string channelId;

            channelId = "SportKlub HD";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 294;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
            }

            channelId = "SportKlub 1";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 399;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
            }

            channelId = "SportKlub 2";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 400;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
            }

            channelId = "SportKlub 3";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 401;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
            }

            channelId = "SportKlub 4";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 240;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
            }

            channelId = "SportKlub 5";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 241;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
            }

            channelId = "SportKlub 6";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 242;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId, 1);
            }

            channelId = "Arena Sport 1";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 179;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            channelId = "Arena Sport 6";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 533;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            channelId = "Food Network";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 265;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml);
            }

            channelId = "HBO";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 366;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml);
            }

            channelId = "HBO 2";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 367;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml);
            }

            channelId = "HBO 3";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 368;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml);
            }

            channelId = "EuroSport 1";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 493;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            channelId = "EuroSport 2";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 494;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            channelId = "Jugoton";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 308;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            channelId = "National Geographic";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 48;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            //channelId = "MTV Base";
            //if (!epgXml.ExistsProgrammeForChannel(channelId))
            //{
            //    var mojTvId = 45;
            //    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            //}

            //channelId = "MTV Hits";
            //if (!epgXml.ExistsProgrammeForChannel(channelId))
            //{
            //    var mojTvId = 46;
            //    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            //}

            //channelId = "MTV Adria";
            //if (!epgXml.ExistsProgrammeForChannel(channelId))
            //{
            //    var mojTvId = 447;
            //    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            //}

            //channelId = "VH1 Classic";
            //if (!epgXml.ExistsProgrammeForChannel(channelId))
            //{
            //    var mojTvId = 133;
            //    epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            //}

            channelId = "HNTV";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 524;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            channelId = "RTL";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 4;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            channelId = "Discovery Science";
            if (!epgXml.ExistsProgrammeForChannel(channelId))
            {
                var mojTvId = 59;
                epgXml = await fetchMojTvProgrammeForChannel(mojTvId, epgXml, channelId);
            }

            return epgXml;
        }

        private async Task<XmlEpg> fetchMojTvProgrammeForChannel(int channelId, XmlEpg baseProgrammeAddTo, string newEpgId = null, int? addHoursToProgrammeTime = null)
        {
            try
            {
                var zagrebTime = DateTimeUtils.GetZagrebCurrentDateTime();
                var todayScheduleTask = _epgService.FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id={channelId}&date={zagrebTime.ToString("d.M.yyyy.")}");
                await Task.Delay(TimeSpan.FromSeconds(1));
                var tommorowScheduleTask = _epgService.FetchXmlEpg($"https://mojtv.net/xmltv/service.ashx?kanal_id={channelId}&date={zagrebTime.AddDays(1).ToString("d.M.yyyy.")}");
                await Task.WhenAll(todayScheduleTask, tommorowScheduleTask);

                var fullProgramme = new List<EpgProgramme>();

                fullProgramme.AddRange(todayScheduleTask.Result.Programme);
                fullProgramme.AddRange(tommorowScheduleTask.Result.Programme);

                baseProgrammeAddTo.AddChannel(todayScheduleTask.Result.Channels.First(), fullProgramme);

                if (addHoursToProgrammeTime != null)
                {
                    baseProgrammeAddTo.AddHoursToProgrammeTimeForChannel(todayScheduleTask.Result.Channels.First().ChannelEpgId, (int)addHoursToProgrammeTime);
                }

                if (!string.IsNullOrEmpty(newEpgId))
                {
                    var oldEpgId = todayScheduleTask.Result.Channels.First().ChannelEpgId;
                    baseProgrammeAddTo.ChangeEpgIdForChannel(oldEpgId, newEpgId);
                }

                return baseProgrammeAddTo;
            }
            catch (Exception ex)
            {
                _epgService.Logger.LogInformation($"MOJ TV ERROR: {ex.Message}");
                return baseProgrammeAddTo;
            }
        }
    }
}
