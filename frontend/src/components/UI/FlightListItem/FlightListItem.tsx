import { useNavigate } from "react-router-dom";
import styles from './FlightListItem.module.css';
import { Flight } from "../../../types/flights";
import { parseToCustomFormat } from "../../../utils/utils";

export interface FlightListItemProps {
	isHeader?: boolean;
	flight?: Flight;
	chooseFlight?: (flightNumber: string) => void;
}

export const FlightListItem: React.FC<FlightListItemProps> = ({isHeader = false, flight, chooseFlight}) => {
	return (
		<>
		{isHeader ? 
		<div className={`${styles.item} ${styles.header}`}>
			<p className={styles.number}>Номер</p>
			<p className={styles.fromAirport}>Из аэропорта</p>
            <p className={styles.toAirport}>В аэропорт</p>
            <p className={styles.date}>Дата</p>
            <p className={styles.price}>Цена</p>
		</div>
		: 
		<li key={flight?.flightNumber} className={styles.item}>
			<div className={styles.number}><span>{flight?.flightNumber}</span></div>
			<div className={styles.fromAirport}><span>{flight?.fromAirport}</span></div>
            <div className={styles.toAirport}><span>{flight?.toAirport}</span></div>
            <div className={styles.date}><span>{parseToCustomFormat(flight!.date)}</span></div>
            <div className={styles.price}><span>{flight?.price}</span></div>
			{chooseFlight &&
			<button className={styles.buy_button} onClick={() => chooseFlight(flight!.flightNumber)}>
				Купить билет
			</button>
			}
		</li>
		}
		</>
	)
}
