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
}