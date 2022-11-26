import "./HomePage.css"
import {useState} from "react";
import StrikeArmyQR from "../Components/StrikeArmyQR";
import {lnurlPay} from "../Util";

export default function HomePage() {
    const [username, setUsername] = useState("");
    const [link, setLink] = useState("");
    const [lightningAddress, setLightningAddress] = useState("");
    const [profile, setProfile] = useState(null);
    const [error, setError] = useState("");
    const [withAvatar, setWithAvatar] = useState(true);

    async function tryGetProfile() {
        try {
            let profileRsp = await fetch(`/profile/${username}`);
            if (profileRsp.ok) {
                return await profileRsp.json();
            }
        } catch (e) {
            console.warn(e.message);
        }

        return null;
    }

    async function updateQrLink() {
        setLink("");
        let profile = await tryGetProfile();
        if (profile === null) {
            setError("No profile found");
            return;
        }

        setLink(lnurlPay(username));
        setLightningAddress(`${username}@${window.location.host}`);
        setProfile(profile);
    }

    let avatar = profile?.avatarUrl?.length > 0 && withAvatar ? `/profile/${username}/avatar` : null;

    return (
        <div className="home-page">
            <div className="m20">
                <h1>LNURL services for Strike users!</h1>
                <div>
                    Enabled Services:
                    <ul>
                        <li>LNURL-Pay</li>
                        <li>Lightning Address</li>
                        <li>LNURL-Withdraw</li>
                    </ul>

                    <div className="btn" onClick={() => window.location.href = "/auth"}>
                        Login with Strike
                    </div>
                </div>
                <h1>QR Generator</h1>
                <div>
                    <div className="flex qr-input flex-align-center">
                        <div>
                            <input type="text" placeholder="Strike username" value={username}
                                   onChange={e => setUsername(e.target.value)}/>
                        </div>
                        <div className="btn" onClick={_ => updateQrLink()}>Generate</div>
                        <div className="flex flex-align-center">
                            <div>
                                <input type="checkbox" checked={withAvatar}
                                       onChange={e => setWithAvatar(e.target.checked)}/>
                            </div>
                            <div className="flex-grow">With Avatar</div>
                        </div>
                    </div>
                    {link.length > 0 ?
                        <>
                            <StrikeArmyQR link={link} avatar={avatar}/>
                            <dl>
                                <dt>LNURL:</dt>
                                <dd><code>{link}</code></dd>
                                <dt>Lightning Address:</dt>
                                <dd><a href={`lightning:${lightningAddress}`}>{lightningAddress}</a></dd>
                                <dt>Tip Page:</dt>
                                <dd><a target="_blank" href={`https://strike.me/${username}`}>{username}</a></dd>
                            </dl>
                        </> : null}
                    {error.length > 0 ? <h3 className="error">{error}</h3> : null}
                </div>

                <h1>FAQ</h1>
                <h2>What is LNURL?</h2>
                <p>
                    LNURL enables services and wallets to dynamically produce and comsume invoices on-demand without the
                    need for manual intervension.
                </p>
                <h2>Why do I need this?</h2>
                <div>
                    You dont! But here a few reasons why you might want to:
                    <ul>
                        <li>Static QR codes for tips</li>
                        <li>Non-Strike users can send you funds instantly without needing you to generate an invoice
                        </li>
                        <li>Invoice expiry is no longer a problem because invoices are generated on demand</li>
                    </ul>
                </div>
                <h2>How can I trust this?</h2>
                <p>
                    Trust is not required, look at the invoices generated and you can see funds are received directly
                    to strike's lightning nodes.
                    Paste the invoice into <a href="https://lightningdecoder.com/"
                                              target="_blank">lightningdecoder.com</a> and lookup the
                    "Payee Pub Key" on <a href="https://amboss.space/" target="_blank">amboss.space</a> it will say
                    lndX.zaphq.io
                </p>
                <h2>How does this work?</h2>
                <p>
                    Build using <a href="https://docs.strike.me" target="_blank">Strike Api</a>
                </p>
            </div>
        </div>
    );
}