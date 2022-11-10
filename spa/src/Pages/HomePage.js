import "./HomePage.css"
import {useState} from "react";
import StrikeArmyQR from "../Components/StrikeArmyQR";
import {bech32} from "bech32";

export default function HomePage() {
    const [username, setUsername] = useState("");
    const [link, setLink] = useState("");
    const [lightningAddress, setLightningAddress] = useState("");

    function updateQrLink() {
        let link = `https://${window.location.host}/pay/${username}`;

        let words = new TextEncoder().encode(link);
        let lnurl = bech32.encode("lnurl", bech32.toWords(words));
        setLink(lnurl.toUpperCase());
        setLightningAddress(`${username}@${window.location.host}`);
    }

    return (
        <div className="home-page">
            <h1>LNURL services for Strike users!</h1>
            <p>
                Enabled Services:
                <ul>
                    <li>LNURL-Pay</li>
                    <li>Lightning Address</li>
                    <li>LNURL-Withdraw <small>(Coming soon!)</small></li>
                </ul>
            </p>
            <h1>QR Generator</h1>
            <p>
                <input type="text" placeholder="Strike username" value={username}
                       onChange={e => setUsername(e.target.value)}/>
                <div className="btn" onClick={_ => updateQrLink()}>Generate</div>
                {link.length > 0 ?
                    <>
                        <StrikeArmyQR link={link}/>
                        <dl>
                            <dt>LNURL:</dt>
                            <dd><code>{link}</code></dd>
                            <dt>Lightning Address:</dt>
                            <dd><a href={`lightning:${lightningAddress}`}>{lightningAddress}</a></dd>
                        </dl>
                    </> : null}
            </p>

            <h1>FAQ</h1>
            <h2>What is LNURL?</h2>
            <p>
                LNURL enables services and wallets to dynamically produce and comsume invoices on-demand without the
                need for manual intervension.
            </p>
            <h2>Why do I need this?</h2>
            <p>
                You dont! But here a few reasons why you might want to:
                <ul>
                    <li>Static QR codes for tips</li>
                    <li>Non-Strike users can send you funds instantly without needing you to generate an invoice</li>
                    <li>Invoice expiry is no longer a problem because invoices are generated on demand</li>
                </ul>
            </p>
            <h2>How can I trust this?</h2>
            <p>
                Trust is not required, look at the invoices generated and you can see funds are received directly
                to strike's lightning nodes.
                Paste the invoice into <a href="https://lightningdecoder.com/" target="_blank">lightningdecoder.com</a> and lookup the
                "Payee Pub Key" on <a href="https://amboss.space/" target="_blank">amboss.space</a> it will say lndX.zaphq.io
            </p>
            <h2>How does this work?</h2>
            <p>
                Build using <a href="https://docs.strike.me" target="_blank">Strike Api</a>
            </p>
        </div>
    );
}