import styles from './BonusHistoryListItem.module.css';
import { parseToCustomFormat } from "../../../utils/utils";
import { PrivilegeHistory } from "../../../types/privileges";

export interface BonusHistoryListItemProps {
	isHeader?: boolean;
	history?: PrivilegeHistory;
	count?: number;
}

export const BonusHistoryListItem: React.FC<BonusHistoryListItemProps> = ({isHeader = false, history, count}) => {
	return (
		<>
		{isHeader ? 
		<div className={`${styles.item} ${styles.header}`}>
            <p className={styles.date}>Дата</p>
            <p className={styles.diff}>Изменение</p>
			<p className={styles.count}>Количество</p>
		</div>
		: 
		<li key={history?.date} className={styles.item}>
			<div className={styles.date}><span>{parseToCustomFormat(history!.date)}</span></div>
			<div className={styles.diff}><span>{`${history?.operationType === "FILL_IN_BALANCE" ? '+' : '-'}${history?.balanceDiff}`}</span></div>
			<div className={styles.count}><span>{count}</span></div>
		</li>
		}
		</>
	)
}
