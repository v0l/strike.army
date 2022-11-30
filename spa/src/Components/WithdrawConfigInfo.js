import "./WithdrawConfigInfo.css";
import StrikeArmyQR from "./StrikeArmyQR";
import {lnurlWithdraw, lnurlWithdrawUri} from "../Util";

export default function WithdrawConfigInfo(props) {
    const config = props.config;

    const isBolt = config.boltCardConfig !== null && config.boltCardConfig !== undefined;
    const lnurl = isBolt ? lnurlWithdrawUri(config.id) : lnurlWithdraw(config.id);


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
        }
        let newConfig = {
            protocol_name: "create_bolt_card_response",
            protocol_version: 1,
            card_name: config.id,
            lnurlw_base: lnurlWithdrawUri(config.id),
            k0: bc.k0.replaceAll(/-/g, ''),
            k1: bc.k1.replaceAll(/-/g, ''),
            k2: bc.k2.replaceAll(/-/g, ''),
            k3: bc.k3.replaceAll(/-/g, ''),
            k4: bc.k4.replaceAll(/-/g, '')
        }
        console.log(wipeConfig, newConfig);
        return (
            <div>
                <h3>Regular QR</h3>
                <StrikeArmyQR link={lnurl}/>
                <h3>Bolt Card Wipe</h3>
                <StrikeArmyQR link={JSON.stringify(newConfig)}/>
            </div>
        )    
    }
    
    function renderBasic() {
        return (
            <>
                <StrikeArmyQR link={lnurl}/>
                <h3>LNURL Code</h3>
                <code>{lnurl}</code>
            </>
        );
    }

    return (
        <div className="qr-info">
            {isBolt ? renderBolt() : renderBasic()}
        </div>
    )
}