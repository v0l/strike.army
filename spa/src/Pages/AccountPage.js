import "./AccountPage.css";
import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
import StrikeModal from "../Components/StrikeModal";
import NewWithdrawConfig from "../Components/WithdrawConfigInput";
import StrikeArmyQR from "../Components/StrikeArmyQR";
import {bech32} from "bech32";

export default function AccountPage() {
    const navigate = useNavigate();
    const [account, setAccount] = useState(null);
    const [showModal, setShowModal] = useState(false);
    const [showConfigQr, setShowConfigQr] = useState(null);

    async function tryLoadAccount() {
        let rsp = await fetch("/user");
        if (rsp.ok) {
            setAccount(await rsp.json());
        } else {
            navigate("/");
        }
    }

    function renderModal() {
        if (!showModal) {
            return null;
        }

        return (
            <StrikeModal close={() => setShowModal(false)}>
                <NewWithdrawConfig/>
            </StrikeModal>
        );
    }

    function renderWithdrawConfig(cfg) {
        return <tr key={cfg.id}>
            <td>{cfg.id}</td>
            <td>{cfg.type}</td>
            <td>0%</td>
            <td>
                <div className="btn btn-small" onClick={() => setShowConfigQr(cfg)}>QR</div>
                <div className="btn btn-small">Delete</div>
            </td>
        </tr>
    }

    function renderWithdrawConfigQr() {
        if (showConfigQr === null) return null;

        let link = `https://${window.location.host}/withdraw/${showConfigQr.id}`;

        let words = new TextEncoder().encode(link);
        let lnurl = bech32.encode("lnurl", bech32.toWords(words), 10_000);

        return (
            <StrikeModal close={() => setShowConfigQr(null)}>
                <div className="qr-info">
                    <StrikeArmyQR link={lnurl.toUpperCase()}/>
                </div>
            </StrikeModal>
        );
    }

    useEffect(() => {
        tryLoadAccount();
    }, []);

    if (!account) return null;

    return (
        <>
            <div className="account-page">
                <h1>Welcome back, {account.profile.handle}!</h1>
                <h2>Balances</h2>
                {account.balances.map(b =>
                    <div className="balance" key={b.currency}>
                        <div>{b.currency}</div>
                        <div>
                            {b.available.toLocaleString("en-US", {
                                maximumFractionDigits: 8
                            })}
                        </div>
                    </div>)}
                <h2>Withdraw Configs</h2>
                <div className="btn" onClick={_ => setShowModal(true)}>+Add</div>
                <br/><br/>
                <table>
                    <thead>
                    <tr>
                        <th>K1</th>
                        <th>Type</th>
                        <th>Usage</th>
                        <th>Actions</th>
                    </tr>
                    </thead>
                    <tbody>
                    {account.user.withdrawConfigs.map(a => renderWithdrawConfig(a))}
                    </tbody>
                </table>
            </div>
            {renderModal()}
            {renderWithdrawConfigQr()}
        </>
    );
}