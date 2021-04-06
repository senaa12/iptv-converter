import React from 'react';
import { BrowserRouter, Switch, Route } from 'react-router-dom';
import Home from './home/home';

const app: React.FC = () => {
    return (
        <BrowserRouter>
            <Switch>
                <Route path="/" component={Home} />
            </Switch>
        </BrowserRouter>           
    );
};

export default app;