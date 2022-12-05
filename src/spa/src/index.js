import './index.css';
import React from 'react';
import ReactDOM from 'react-dom/client';
import {Provider} from "react-redux";
import {BrowserRouter as Router, Route, Routes} from "react-router-dom";
import {ReduxStore} from "./State/Store"

import HomePage from "./Pages/HomePage";
import AccountPage from "./Pages/AccountPage";
import TipPage from "./Pages/TipPage";

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
    <React.StrictMode>
        <Provider store={ReduxStore}>
            <Router>
                <Routes>
                    <Route exact path="/" element={<HomePage/>}/>
                    <Route exact path="/account" element={<AccountPage/>}/>
                    <Route path="/:username" element={<TipPage/>}/>
                </Routes>
            </Router>
        </Provider>
    </React.StrictMode>
);
