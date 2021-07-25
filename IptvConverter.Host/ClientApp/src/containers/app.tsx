import React from 'react';
import ApplicationWizard from './applicationWizard/applicationWizard';
import Mobile, { Desktop } from '../components/mobile';
import MobileView from './mobileView/mobileView';

import './app.scss';

const app: React.FC = () => {
    return (
        <div>
            <Mobile>
                <MobileView />
            </Mobile>
            <Desktop>
                <ApplicationWizard />
            </Desktop>
        </div>
    )
};

export default app;