import styles from './TicketListItem.module.css';
import { parseToCustomFormat } from "../../../utils/utils";
import { Ticket } from '../../../types/tickets';

export interface TicketListItemProps {
	isHeader?: boolean;
	ticket?: Ticket;
	cancel?: (ticketUid: string) => void;
}

export const TicketListItem: React.FC<TicketListItemProps> = ({isHeader = false, ticket, cancel}) => {
	return (
		<>
		{isHeader ? 
		<div className={`${styles.item} ${styles.header}`}>
			<p className={styles.number}>Номер рейса</p>
			<p className={styles.fromAirport}>Из аэропорта</p>
            <p className={styles.toAirport}>В аэропорт</p>
            <p className={styles.date}>Дата</p>
            <p className={styles.price}>Цена</p>
		</div>
		: 
		<li key={ticket?.flightNumber} className={styles.item}>
			<div className={styles.number}><span>{ticket?.flightNumber}</span></div>
			<div className={styles.fromAirport}><span>{ticket?.fromAirport}</span></div>
            <div className={styles.toAirport}><span>{ticket?.toAirport}</span></div>
            <div className={styles.date}><span>{parseToCustomFormat(ticket!.date)}</span></div>
            <div className={styles.price}><span>{ticket?.price}</span></div>
			{(cancel && ticket!.status == "PAID") ?
			<button className={styles.buy_button} onClick={() => cancel(ticket!.ticketUid)}>
				Вернуть билет
			</button>
            :
            (
                ticket!.status == "CANCELED" &&
                <span>Возвращён</span>
            )
			}
		</li>
		}
		</>
	)
}
