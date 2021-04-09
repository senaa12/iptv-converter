import React, { useCallback, useState } from 'react';
import FileInput from '../components/input/fileInput';
import Button from '../components/button/button';
import { IptvChannelsExtendedWithState } from '../models/channels';
import { IptvChannel, PlaylistClient } from '../services/services';
import Table from './table';
import SliderMenu from '../components/sliderMenu/sliderMenu';
import Loading from '../components/loading/loading';

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
    const [ generateLoading, setGenerateLoading ] = useState(false);
    const [ channels, setChannels ] = useState<Array<IptvChannelsExtendedWithState> | undefined>();

    const readPlaylistCallback = useCallback(
        async (fillData: boolean = true) => {
            setLoading(true);

            const playlistClient = new PlaylistClient();
            const result = await playlistClient.preview(fillData, { fileName: (file as any).name, data: file as any });

            setChannels(result.data?.map((val, index) => {
                return new IptvChannelsExtendedWithState({ channelUniqueId: index, includeInFinal: (val.recognized && val.hd) ?? false, ...val });
            }));

            setLoading(false);
        },
        [setLoading, setChannels, file]
    )

    const readWithStepChange = useCallback(
        (fillData: boolean) => () => {
            readPlaylistCallback(fillData);
            selectStep(steps[1]);
        },
        [selectStep, readPlaylistCallback]
    )

    const onGeneratePlayListClick = useCallback(
        async () => {
            setGenerateLoading(true);

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

            setGenerateLoading(false);

        },
        [setGenerateLoading, channels]
    )

    const filterOutNotSelected = useCallback(
        () => {
            setLoading(true);

            console.log(channels)
            localStorage.setItem('channels', JSON.stringify(channels?.map(x => new IptvChannelsExtendedWithState(x))));
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
                selectStep(steps[1])
                await readPlaylistCallback();
            }
            if(step.id === 2) {
                selectStep(steps[2]);
                filterOutNotSelected();
            }
            if(step.id === 3) {
                selectStep(steps[0]);
                setFile(undefined);
                await onGeneratePlayListClick();
            }
        },
        [step, readPlaylistCallback, selectStep, filterOutNotSelected, onGeneratePlayListClick, setFile]
    )

    const previousStepCallback = useCallback(
        () => {
            if(step.id === 2) {
                selectStep(steps[0]);
            }

            if(step.id === 3) {
                setChannels(JSON.parse(localStorage.getItem('channels')!));
                selectStep(steps[1])
            }
        },
        [selectStep, selectStep, step]
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

    const scrollToStart = useCallback(
        () => {
            window.scrollTo({ top: 0, left: 0, behavior: 'smooth' })
        },
        []
    )

    const selectAllChannels = useCallback(
        () => {
            setChannels([ ...channels?.map(x => new IptvChannelsExtendedWithState({ ...x, includeInFinal: true }))! ]);
        },
        [setChannels, channels]
    )

    return (
        <div className={'home'} style={{ overflowY: 'auto', minHeight: 500 }}>
            <div style={{ display: 'flex', height: 80 }}>
                <Button
                    loading={loading}
                    style={{ display: 'inline-flex', position: 'relative', marginTop: 10, marginRight: 30, marginBottom: 30 }}
                    textual={true}
                    onClick={previousStepCallback}
                    disabled={step.id === 1}
                >Back</Button>
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
                    style={{ display: 'inline-flex', position: 'relative', marginTop: 10, marginLeft: 30, marginBottom: 30 }}
                    textual={true}
                    onClick={nextStepCallback}
                    disabled={step.id === 3 || !file || step.id === 1}
                >Next</Button>
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', paddingBottom: 70, minHeight: 500, width: '100%' }}>
                {step.id === 1 && <>
                    <FileInput onChange={setFile} accept={'audio/x-mpegurl'}/>
                    <div style={{ display: 'flex' }}>
                        <Button 
                            disabled={!file}
                            onClick={readWithStepChange(false)}
                            style={{ margin: 20 }}
                            loading={loading}
                        >IMPORT</Button>
                        <Button
                            disabled={!file}
                            onClick={readWithStepChange(true)}
                            style={{ margin: 20 }}
                            loading={loading}
                        >IMPORT WITH PROCESS</Button>
                    </div>
                </>}
                {(step.id === 2 || step.id === 3) && 
                    ((!loading && !generateLoading) ?
                        <>
                            <Button
                                loading={generateLoading}
                                style={{ display: 'inline-flex', marginBottom: 30 }}
                                onClick={nextStepCallback}
                                textual={true}
                                disabled={step.id !== 3}
                            >Generate Playlist</Button>
                            <Table 
                            filter={step.id === 2}
                            items={channels!}
                            setItems={setChannels}
                            toggleCheck={toggleCheck}
                            epgNameChange={epgNameChange}
                            groupNameChange={groupNameChange}
                        />
                        </> : <Loading />)
                }
            </div>
            {step.id !== 1 && <div style={{ position: 'fixed', bottom: 25, right: 25, display: 'flex' }}>
                {step.id === 2 && <Button
                    style={{ margin: 10 }}
                    onClick={selectAllChannels}
                    textual={true}
                >Include All</Button>}
                <Button
                    style={{ margin: 10 }}
                    onClick={scrollToStart}
                    textual={true}
                >Scroll To Start</Button>
            </div>}
        </div>
    )
};

export default app;