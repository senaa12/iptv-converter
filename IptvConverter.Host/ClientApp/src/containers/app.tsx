import React, { useCallback, useState } from 'react';
import FileInput from '../components/input/fileInput';
import Button from '../components/button/button';
import { IptvChannelsExtendedWithState } from '../models/channels';
import { IptvChannel, PlaylistClient } from '../services/services';
import Table from './table';
import SliderMenu from '../components/sliderMenu/sliderMenu';

import './app.scss';

const steps = [
    {
        id: 1,
        name: 'Step 1: Select file'
    },
    {
        id: 2,
        name: 'Step 2: Filter Channels'
    },
    {
        id: 3,
        name: 'Step 3: Order Channels'
    }
]

const app: React.FC = () => {
    const [ step, selectStep ] = useState<any>(steps[0]);
    const [ file, setFile ] = useState<File | undefined>();
    const [ loading, setLoading ] = useState(false);
    const [ channels, setChannels ] = useState<Array<IptvChannelsExtendedWithState> | undefined>();

    const readPlaylistCallback = useCallback(
        async () => {
            setLoading(true);

            const playlistClient = new PlaylistClient();
            const result = await playlistClient.preview({ fileName: (file as any).name, data: file as any });

            setChannels(result.data?.map((val, index) => {
                return new IptvChannelsExtendedWithState({ channelUniqueId: index, includeInFinal: (val.recognized && val.hd) ?? false, ...val });
            }));

            setLoading(false);
        },
        [setLoading, setChannels, file]
    )

    const onGeneratePlayListClick = useCallback(
        async () => {
            setLoading(true);

            const channelsToGenerate = channels?.filter(x => x.includeInFinal === true).map(x => ( 
                new IptvChannel({ epgId: x.epgId, extInf: x.extInf, group: x.group, id: x.id, logo: x.logo, name: x.name, uri: x.uri }) 
            ))

            const playlistClient = new PlaylistClient();
            const fileResult = await playlistClient.channels(channelsToGenerate);

            let a = document.createElement("a") 
            let blobURL = URL.createObjectURL(fileResult.data)
            a.download = fileResult.fileName ?? 'download';
            a.href = blobURL
            document.body.appendChild(a)
            a.click()
            document.body.removeChild(a)

            setLoading(false);

        },
        [setLoading, channels]
    )

    const filterOutNotSelected = useCallback(
        () => {
            setLoading(true);

            setChannels([
                ...channels?.filter(x => x.includeInFinal).map(x => ( new IptvChannelsExtendedWithState(x) ))!
            ])

            setLoading(false);
        },
        [channels, setChannels, setLoading]
    )

    const nextStepCallback = useCallback(
        async () => {
            if(step.id === 1) {
                await readPlaylistCallback();
                selectStep(steps[1])
            }
            if(step.id === 2) {
                filterOutNotSelected();
                selectStep(steps[2]);
            }
            if(step.id === 3) {
                await onGeneratePlayListClick();
                setFile(undefined);
                selectStep(steps[0]);
            }
        },
        [step, readPlaylistCallback, selectStep, filterOutNotSelected, onGeneratePlayListClick, setFile]
    )

    const toggleCheck = useCallback(
        (id: number) => (newVal: boolean) => {
            setChannels([ ...channels?.map(x => {
                    if(x.channelUniqueId === id) {
                        return new IptvChannelsExtendedWithState({ ...x!, includeInFinal: newVal });
                    }
                    else {
                        return  new IptvChannelsExtendedWithState(x);
                    }
                })! 
            ])
        },
        [channels, setChannels]
    )

    const groupNameChange = useCallback(
        (id: number) => (e: any) => {
            setChannels([ ...channels?.map(x => {
                if(x.channelUniqueId === id) {
                    return new IptvChannelsExtendedWithState({ ...x!, group: e.target.value });
                }
                else {
                    return  new IptvChannelsExtendedWithState(x);
                }
            })! 
            ])
        },
        [setChannels, channels]
    )

    const epgNameChange = useCallback(
        (id: number) => (e: any) => {
            setChannels([ ...channels?.map(x => {
                if(x.channelUniqueId === id) {
                    return new IptvChannelsExtendedWithState({ ...x!, epgId: e.target.value });
                }
                else {
                    return  new IptvChannelsExtendedWithState(x);
                }
            })! 
            ])
        },
        [channels, setChannels]
    );

    return (
        <div className={'home'}>
            <div style={{ display: 'flex', height: 70 }}>
                <SliderMenu 
                    options={steps}
                    getOptionLabel={x => x.name}
                    getOptionId={x => x.id}
                    selectedOption={step}
                    onSelect={() => {}}
                    width={750}
                />
                <Button
                    loading={loading}
                    style={{ display: 'inline-flex', paddingTop: 20, marginLeft: 20, paddingBottom: 20, textDecoration: 'underline' }}
                    textual={true}
                    onClick={nextStepCallback}
                    // disabled={step.id === 3}
                >Next</Button>
                {/* {step.id === 3 && 
                    <Button
                        loading={loading}
                        style={{ margin: '10px 10px 20px 10px' , padding: '3px 15px' }}
                        onClick={nextStepCallback}
                    >Generate File</Button>
                } */}
            </div>
            {step.id === 1 && <FileInput onChange={setFile} accept={'audio/x-mpegurl'}/>}
            {(step.id === 2 || step.id === 3) && (
                <Table 
                    filter={step.id === 2}
                    items={channels!}
                    setItems={setChannels}
                    toggleCheck={toggleCheck}
                    epgNameChange={epgNameChange}
                    groupNameChange={groupNameChange}
                />
            )}
        </div>
    )
};

export default app;