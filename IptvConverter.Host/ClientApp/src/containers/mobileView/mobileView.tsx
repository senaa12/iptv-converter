import React, { useCallback, useState } from 'react';
import { AppClient } from '../../services/services';
import Button from '../../components/button/button';

import './mobileView.scss';

const mobileView = () => {
    const [ loading, setLoading ] = useState(false);
    const [ infoMsg, setInfoMsg ] = useState<string | undefined>();

    const onFetchCallback = useCallback(
        async () => {
            setLoading(true);
            setInfoMsg(undefined);

            try {
                const appClient = new AppClient();
                await appClient.generate(true);
                setInfoMsg('EPG generated')
            } 
            catch {
                setInfoMsg('Error while generating EPG');
            }

            setLoading(false);
        },
        [setInfoMsg, setLoading]
    )

    return (
        <div className="mobile-view">
            <div className="mobile-view__label">
                APPLICATION NOT AVAILABLE IN MOBILE
            </div>
            <div className="mobile-view__button">
                <div>
                <Button
                    loading={loading}
                    onClick={onFetchCallback}
                >
                    GENERATE EPG
                </Button>
                </div>
                {infoMsg && <div style={{ marginTop: 15 }}>{infoMsg}</div>}
            </div>
        </div>
    )
}

export default mobileView;