import React from 'react';
import ReactDOM from 'react-dom/client';
import {Provider} from "react-redux";

import './index.css';
import {BrowserRouter as Router, Route, Routes} from "react-router-dom";
import HomePage from "./Pages/HomePage";
import {ReduxStore} from "./State/Store"

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
    <React.StrictMode>
        <Provider store={ReduxStore}>
            <Router>
                <Routes>
                    <Route exact path="/" element={<HomePage/>}/>    
                </Routes>
            </Router>
        </Provider>
    </React.StrictMode>
);