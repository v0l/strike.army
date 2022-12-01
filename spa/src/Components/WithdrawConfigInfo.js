import "./WithdrawConfigInfo.css";
import StrikeArmyQR from "./StrikeArmyQR";
import {lnurlWithdraw} from "../Util";
import {useEffect, useState} from "react";

export default function WithdrawConfigInfo(props) {
    const config = props.config;
    const [showNewCard, setShowNewCard] = useState(false);
    const [showWipeCard, setShowWipeCard] = useState(false);
    const [setupKey, setSetupKey] = useState("");

    const isBolt = config.boltCardConfig !== null && config.boltCardConfig !== undefined;

    async function getNewBoltCardCode() {
        let req = await fetch(`/user/withdraw-config/${config.id}/bolt-card-setup`);
        if (req.ok) {
            setSetupKey(await req.json());
        }
    }

    function renderBolt() {
        let bc = config.boltCardConfig;
        let wipeConfig = {
            version: 1,
            action: "wipe",
            k0: bc.k0.replaceAll(/-/g, ''),
            k1: bc.k1.replaceAll(/-/g, ''),
            k2: bc.k2.replaceAll(/-/g, ''),
            k3: bc.k3.replaceAll(/-/g, ''),
            k4: bc.k4.replaceAll(/-/g, '')
        };
        let setupLink = `https://${window.location.host}/user/bolt-card-setup/${setupKey}`;
        let lnurl = lnurlWithdraw(config.id);
        return (
            <div>
                <h3>Regular QR</h3>
                <StrikeArmyQR link={lnurl}/>
                <code>{lnurl}</code>
                <br/>
                {showNewCard ? <>
                        <h3>New Bolt Card QR</h3>
                        <StrikeArmyQR link={setupLink}/>
                    </> :
                    <div className="btn btn-small m5" onClick={() => setShowNewCard(true)}>Show New Bolt Card QR</div>}
                {showWipeCard ? <>
                    <h3>Wipe Bolt Card QR</h3>
                    <StrikeArmyQR link={JSON.stringify(wipeConfig)}/>
                </> : <div className="btn btn-small m5" onClick={() => setShowWipeCard(true)}>Show Wipe QR</div>}
            </div>
        )
    }

    function renderBasic() {
        let lnurl = lnurlWithdraw(config.id);
        return (
            <>
                <StrikeArmyQR link={lnurl}/>
                <h3>LNURL Code</h3>
                <code>{lnurl}</code>
            </>
        );
    }

    useEffect(() => {
        if (config !== null && showNewCard === true) {
            let code = config?.boltCardConfig?.setupKey;
            if (code === null || code === undefined) {
                getNewBoltCardCode();
            } else {
                setSetupKey(code);
            }
        }
    }, [showNewCard]);
    
    return (
        <div className="qr-info">
            {isBolt ? renderBolt() : renderBasic()}
        </div>
    )
}