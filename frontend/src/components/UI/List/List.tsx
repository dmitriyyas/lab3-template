import styles from './List.module.css'

export interface ListProps<T extends React.ReactNode> {
	title?: string;
	header: T;
	items: T[];
	navigation?: React.ReactNode;
	isScrollable?: boolean;
	isExpandable?: boolean;
}

export const List = <T extends React.ReactNode,>({title, header, items, navigation, isScrollable = false, isExpandable = false}: ListProps<T>) => {
	return (
		<div className={styles.list_container}>
			{title && <p className={styles.list_name}>{title}</p>}
			{navigation && 
				<div className={styles.navigation_container}>
					{navigation}
				</div>
			}
			{navigation && <hr/>}
			<div className={styles.list_header}>
				{header}
			</div>
			{!navigation && <hr/>}
			<ul className={`${styles.list} ${isScrollable ? styles.scrollable : ''} ${isExpandable ? styles.expandable : ''}`}>
				{items}
			</ul>
		</div>
	)
}
