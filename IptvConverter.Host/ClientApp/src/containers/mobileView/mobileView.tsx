import React, { useCallback, useEffect, useState } from 'react';
import { AppClient } from '../../services/services';
import Button from '../../components/button/button';
import { registerPeriodicEpgGeneration } from '../../utilities/serviceWorker';

import './mobileView.scss';

const mobileView = () => {
    const [ loading, setLoading ] = useState(false);
    const [ infoMsg, setInfoMsg ] = useState<string | undefined>();

    const [ registrationLoading, setRegistrationLoading ] = useState(false);
    const [ registrationInfoMsg, setRegistrationInfoMsg ] = useState<string | undefined>();

    const [ lastGenerationDateTime, setLastGenerationDateTime ] = useState<string | undefined>();

    const fetchLastGenerationTime = useCallback(async() => {
            const client = new AppClient();
            const result = (await client.lastGenerationTime()).data;
            if(!result){
                setLastGenerationDateTime('-')
            }

            const dateResult = new Date(result!);
            setLastGenerationDateTime(`${dateResult.toLocaleDateString()} ${dateResult.toLocaleTimeString()}`);
        }, 
        [setLastGenerationDateTime]
    )

    useEffect(
        () => {
            fetchLastGenerationTime();
        },
        []
    )

    const onFetchCallback = useCallback(
        async () => {
            setLoading(true);
            setInfoMsg(undefined);
            setRegistrationInfoMsg(undefined);

            try {
                const appClient = new AppClient();
                await appClient.generate(true);
                setInfoMsg('EPG generated');

                setLastGenerationDateTime(undefined);
                fetchLastGenerationTime();
            } 
            catch (e) {
                setInfoMsg(`Error while generating EPG: ${e}`);
            }

            setLoading(false);
        },
        [setInfoMsg, setLoading, setRegistrationInfoMsg, setLastGenerationDateTime, fetchLastGenerationTime]
    )

    const onRegistrationCallback = useCallback(
        async () => {
            setRegistrationLoading(true);
            setInfoMsg(undefined);
            setRegistrationInfoMsg(undefined);

            try {
                const result = await registerPeriodicEpgGeneration();
                setRegistrationInfoMsg(result);
            } 
            catch (e) {
                setRegistrationInfoMsg(e);
            }

            setRegistrationLoading(false);
        },
        [setInfoMsg, setRegistrationInfoMsg, setRegistrationLoading]
    )

    return (
        <div className="mobile-view">
            <div className="mobile-view__header">
                <span style={{ fontSize: 25 }}>IPTV EPG GENERATOR</span>
                <br />
                <span>LAST GENERATION TIME: {lastGenerationDateTime}</span>
            </div>
            <div className="mobile-view__button">
                <div style={{ marginBottom: 45 }}>
                    <Button
                        loading={registrationLoading}
                        onClick={onRegistrationCallback}
                    >
                        REGISTER AUTO GENERATION
                    </Button>
                    {registrationInfoMsg && <div style={{ marginTop: 15 }}>{registrationInfoMsg}</div>}
                </div>
                <div>
                    <Button
                        loading={loading}
                        onClick={onFetchCallback}
                    >
                        GENERATE EPG
                    </Button>
                    {infoMsg && <div style={{ marginTop: 15 }}>{infoMsg}</div>}
                </div>
            </div>
        </div>
    )
}

export default mobileView;