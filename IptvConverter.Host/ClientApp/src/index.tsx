import React from 'react';
import ReactDOM from 'react-dom';
import App from './containers/app';
import registerServiceWorker from './utilities/serviceWorker';

import './index.scss';

ReactDOM.render(
    <App />,
    document.getElementById('app'),
);

registerServiceWorker();