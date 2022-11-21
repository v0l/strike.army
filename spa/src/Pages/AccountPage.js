import "./AccountPage.css";
import {useEffect, useState, Fragment} from "react";
import {useNavigate} from "react-router-dom";

export default function AccountPage() {
    const navigate = useNavigate();
    const [account, setAccount] = useState(null);

    async function tryLoadAccount() {
        let rsp = await fetch("/user");
        if (rsp.ok) {
            setAccount(await rsp.json());
        } else {
            navigate("/");
        }
    }

    useEffect(() => {
        tryLoadAccount();
    }, []);

    if (!account) return null;

    return (
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
            <div className="btn">+Add</div>
            <br/><br/>
            <table>
                <thead>
                <tr>
                    <th>K1</th>
                    <th>Type</th>
                    <th>Useage</th>
                    <th>Actions</th>
                </tr>
                </thead>
            </table>
        </div>
    );
}