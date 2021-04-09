import React, { useCallback } from "react";
import {
  SortableContainer,
  SortableElement,
  SortableHandle
} from "react-sortable-hoc";
import Checkbox from '../components/checkbox/checkbox';
import Input from '../components/input/input';
import Icon, { IconEnum } from '../components/icon';
import { arrayMove } from '../utilities/functions';

const SortableCont = SortableContainer(({ children }) => <tbody>{children}</tbody>);

const SortableItem = SortableElement((props) => <TableRow {...props} />);

const RowHandler = SortableHandle(() => <Icon iconName={IconEnum.More} className={'handler'} style={{ fill: 'var(--white)' }} />);

const TableRow = ({ channel, toggleCheck, epgNameChange, groupNameChange, filter, className }) => {
  return (
    <tr>
      <td>
        <div className="firstElement" style={{ display: 'flex', justifyContent: 'center' }}>
          {!filter && <RowHandler />}
          {filter && <Checkbox 
            checked={channel.includeInFinal}
            handleOnCheckboxChange={toggleCheck(channel.channelUniqueId)}
          />}
        </div>
      </td>
      <td>{channel.name}</td>
      <td>
        <Input 
            type={'text'}
            value={channel.epgId}
            onChange={epgNameChange}
          />
      </td>
      <td>
          <Input 
            type={'text'}
            value={channel.group}
            onChange={groupNameChange}
          />
        </td>
      <td style={{ width: 100 }}>{!!channel.recognized ? 'Yes' : ''}</td>
      <td style={{ width: 100 }}>{!!channel.hd ? 'Yes' : ''}</td>
    </tr>
  );
};

interface MyTableProps {
    items: any[];
    filter: boolean;
    setItems: (newType: any) => void;
    toggleCheck: (id: number) => (value: boolean) => void;
    groupNameChange: (id: number) => (e: any) => void;
    epgNameChange: (id: number) => (e: any) => void;
}

const MyTable = ({items, filter, setItems, toggleCheck, epgNameChange, groupNameChange}: MyTableProps) => {

  const onSortEnd = useCallback(({ oldIndex, newIndex }) => {
    setItems((oldItems) => arrayMove(oldItems, oldIndex, newIndex));
  }, []);

  return (
    <div>
      <table className="table table-dark fixed_header">
        <thead>
          <tr>
            <th>{filter ? 'Include' : ''}</th>
            <th>Name</th>
            <th>Epg</th>
            <th style={{ width: 150 }}>Group</th>
            <th style={{ width: 100 }}>Recognized</th>
            <th style={{ width: 100 }}>HD</th>
          </tr>
        </thead>
        <SortableCont
          onSortEnd={onSortEnd}
          axis="y"
          lockAxis="y"
          lockToContainerEdges={true}
          lockOffset={["30%", "50%"]}
          helperClass="helperContainerClass"
          useDragHandle={true}
        >
          {items?.map((value, index) => (
            <SortableItem
              key={`item-${index}`}
              index={index}
              channel={value}
              toggleCheck={toggleCheck}
              epgNameChange={epgNameChange(value.channelUniqueId)}
              groupNameChange={groupNameChange(value.channelUniqueId)}
              filter={filter}
            />))}
        </SortableCont>
      </table>
    </div>
  );
};

export default MyTable;