import React from 'react';
import { BrowserRouter, Switch, Link } from 'react-router-dom';

const app: React.FC = () => {
    return (
        <BrowserRouter>
            <Switch>
                <Link to="/" style={{ margin: '0 5px' }}>Home</Link>
            </Switch>
        </BrowserRouter>           
    );
};

export default app;