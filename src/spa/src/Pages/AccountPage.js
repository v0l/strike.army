import "./AccountPage.css";
import {useEffect, useState} from "react";
import {useNavigate} from "react-router-dom";
import StrikeModal from "../Components/StrikeModal";
import NewWithdrawConfig from "../Components/NewWithdrawConfig";
import StrikeArmyQR from "../Components/StrikeArmyQR";
import {lnurlPay} from "../Util";
import WithdrawConfigInfo from "../Components/WithdrawConfigInfo";

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
                <NewWithdrawConfig close={async () => {
                    await tryLoadAccount();
                    setShowModal(false);
                }} minPayment={account?.minPayment}/>
            </StrikeModal>
        );
    }

    async function deleteConfig(cfg) {
        if (window.confirm(`Are you sure you want to delete config: ${cfg.id}`)) {
            await fetch(`/user/withdraw-config/${cfg.id}`, {
                method: "DELETE"
            });
            await tryLoadAccount();
        }
    }

    function renderWithdrawConfig(cfg) {
        let usage = 1 - cfg.remaining / (cfg.type === "SingleUse" ? cfg.max : cfg.configReusable.limit);
        return <tr key={cfg.id}>
            <td>{cfg.description.substring(0, 50)}</td>
            <td>{cfg.type}</td>
            <td>{(usage * 100).toFixed(0)}%</td>
            <td>
                <div className="btn btn-small" onClick={() => setShowConfigQr(cfg)}>QR</div>
                <div className="btn btn-small" onClick={() => deleteConfig(cfg)}>Delete</div>
            </td>
        </tr>
    }

    function renderWithdrawConfigQr() {
        if (showConfigQr === null) return null;

        return (
            <StrikeModal close={() => setShowConfigQr(null)}>
                <WithdrawConfigInfo config={showConfigQr}/>
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
                <div className="m20">
                    <h1>Welcome back, {account.profile.handle}!</h1>
                    <div className="flex">
                        <div>
                            <h2>Balances:</h2>
                            {account.balances.map(b =>
                                <div className="balance" key={b.currency}>
                                    <div>{b.currency}</div>
                                    <div>
                                        {b.available.toLocaleString("en-US", {
                                            maximumFractionDigits: 8
                                        })}
                                    </div>
                                </div>)}
                        </div>
                        <div className="mob-hide">
                            <h2>Receive QR</h2>
                            <StrikeArmyQR link={lnurlPay(account.profile.handle)}></StrikeArmyQR>
                        </div>
                    </div>
                    <h2>Withdraw Configs:</h2>
                    <div className="btn" onClick={_ => setShowModal(true)}>+Add</div>
                    <br/><br/>
                    <table>
                        <thead>
                        <tr>
                            <th>Description</th>
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
            </div>
            {renderModal()}
            {renderWithdrawConfigQr()}
        </>
    );
}