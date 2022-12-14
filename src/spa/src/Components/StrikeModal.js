import "./StrikeModal.css";

export default function StrikeModal(props) {
    const close = typeof props.close === "function" ? props.close : () => console.error("No close function set");
    return (
        <div className="modal-bg" onClick={close}>
            <div className="modal" onClick={e => e.stopPropagation()}>
                {props.children}
            </div>
        </div>
    )
}