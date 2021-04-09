import { IIptvChannelExtended, IptvChannelExtended } from "../services/services";

interface IIptvChannelsExtendedWithState extends IIptvChannelExtended {
    channelUniqueId: number;
    includeInFinal: boolean;
}

export class IptvChannelsExtendedWithState extends IptvChannelExtended implements IIptvChannelsExtendedWithState {
    channelUniqueId: number;
    public includeInFinal: boolean;


    constructor({ channelUniqueId, includeInFinal, ...data }: IIptvChannelsExtendedWithState) {
        super(data);
        this.channelUniqueId = channelUniqueId;
        this.includeInFinal = includeInFinal;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["includeInFinal"] = this.includeInFinal;
        data["channelUniqueId"] = this.channelUniqueId;
        data["id"] = this.id;
        data["epgId"] = this.epgId;
        data["group"] = this.group;
        data["logo"] = this.logo;
        data["extInf"] = this.extInf;
        data["uri"] = this.uri;
        data["shouldCollect"] = this.shouldCollect;
        data["recognized"] = this.recognized;
        data["pattern"] = this.pattern;
        data["country"] = this.country;
        data["hd"] = this.hd;
        return data; 
    }
}