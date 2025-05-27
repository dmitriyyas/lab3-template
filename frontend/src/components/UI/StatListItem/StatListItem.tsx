import { useNavigate } from "react-router-dom";
import styles from './StatListItem.module.css';
import { Flight } from "../../../types/flights";
import { parseToCustomFormat } from "../../../utils/utils";
import { Stat } from "../../../types/stats";

export interface StatListItemProps {
	isHeader?: boolean;
	stat?: Stat;
}

export const StatListItem: React.FC<StatListItemProps> = ({isHeader = false, stat}) => {
	return (
		<>
		{isHeader ? 
		<div className={`${styles.item} ${styles.header}`}>
			<p className={styles.method}>Метод</p>
			<p className={styles.path}>Путь</p>
            <p className={styles.status}>Код ответа</p>
            <p className={styles.duration}>Длительность в мс</p>
            <p className={styles.date}>Дата</p>
		</div>
		: 
		<li key={stat?.id} className={styles.item}>
			<div className={styles.method}><span>{stat?.method}</span></div>
			<div className={styles.path}><span>{stat?.path}</span></div>
            <div className={styles.status}><span>{stat?.statusCode}</span></div>
            <div className={styles.duration}><span>{stat?.durationMs}</span></div>
            <div className={styles.date}><span>{parseToCustomFormat(stat!.timestamp)}</span></div>
		</li>
		}
		</>
	)
}
