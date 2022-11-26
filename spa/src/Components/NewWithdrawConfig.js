import "./NewWithdrawConfig.css";
import {useState} from "react";

export const ConfigType = {
    SingleUse: 0,
    Reusable: 1
};

export const LimitInterval = {
    Daily: 0,
    Weekly: 1
};

export default function NewWithdrawConfig(props) {
    const fnClose = typeof props.close === "function" ? props.close : () => console.error("no close function bound!");
    const [type, setType] = useState(0);
    const [description, setDescription] = useState("");
    const [min, setMin] = useState(0);
    const [max, setMax] = useState(500_000);
    const [interval, setInterval] = useState(0);
    const [limit, setLimit] = useState(1_000_000);

    async function addConfig() {
        let newConfig = {
            type,
            description,
            min,
            max,
            interval: null,
            limit: null
        };
        if (type === ConfigType.Reusable) {
            newConfig.interval = interval;
            newConfig.limit = limit;
        }

        let rsp = await fetch("/user/withdraw-config", {
            method: "POST",
            body: JSON.stringify(newConfig),
            headers: {
                "content-type": "application/json"
            }
        });
        if (rsp.ok) {
            let rspConfig = await rsp.json();
            fnClose(rspConfig);
        }
    }

    function renderReusable() {
        if (type !== ConfigType.Reusable) return null;

        return (
            <>
                <label htmlFor="interval">Limit Interval</label>
                <select id="interval" value={interval} onChange={e => setInterval(parseInt(e.target.value))}>
                    <option value={LimitInterval.Daily}>Daily</option>
                    <option value={LimitInterval.Weekly}>Weekly</option>
                </select>
                <label htmlFor="limit">Limit Amount</label>
                <input type="number" id="limit" value={limit} onChange={e => setLimit(parseInt(e.target.value))}/>
            </>
        );
    }

    return (
        <div className="new-config" onClick={e => e.stopPropagation()}>
            <h2>New Withdraw Config</h2>
            <b>Notes:</b>
            <ul className="notes">
                <li>All values are in sats</li>
                <li>A min value of 0 will be increased to the smallest payment size, usually 0.01 in fiat</li>
            </ul>
            <div className="input">
                <label htmlFor="type">Type</label>
                <select id="type" value={type} onChange={e => setType(parseInt(e.target.value))}>
                    <option value={ConfigType.SingleUse}>Single Use</option>
                    <option value={ConfigType.Reusable}>Reusuable</option>
                </select>
                <label htmlFor="desc">Description</label>
                <input type="text" id="desc" value={description} onChange={e => setDescription(e.target.value)}/>
                <label htmlFor="min">Minimum</label>
                <input type="number" id="min" min={0} placeholder="0" value={min}
                       onChange={e => setMin(parseInt(e.target.value))}/>
                <label htmlFor="max">Maximum</label>
                <input type="number" id="max" min={0} placeholder="1000" value={max}
                       onChange={e => setMax(parseInt(e.target.value))}/>
                {renderReusable()}
                <div className="btn" onClick={addConfig}>Add</div>
            </div>
        </div>
    )
}